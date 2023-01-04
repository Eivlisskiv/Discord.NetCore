using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;
using System.Collections;
using CommandContext = Discord.Bot.Handlers.CommandContext;
using Discord.Bot.Handlers;
using IResult = Discord.Commands.IResult;
using ILogger = Discord.Bot.Logger.ILogger;
using Discord.Net;
using Discord.Bot.Exceptions;
using System.Text.RegularExpressions;
using System.Text;
using Discord.Utils;
using Discord.Utils.Messages;

namespace Discord.Bot.Handler
{
	public class CommandHandler
	{
		public static Bot Bot => Bot.Instance;
		private static ILogger? Log => Bot?.Log;

		public static readonly Hashtable running = new();

		private static CommandHandler _instance;
		public static CommandService CommandService => _instance.service;

		public static bool RunUser(ulong id)
		{
			if (!running.ContainsKey(id))
			{
				running.Add(id, DateTime.UtcNow);
				return true;
			}

			DateTime dateTime = (DateTime)running[id]!;
			if (dateTime.AddMinutes(2) < DateTime.UtcNow)
			{
				running[id] = DateTime.UtcNow;
				return true;
			}
			
			return false;
		}

		private static bool MessageIsApproved(SocketUserMessage msg, string prefix, out int argPosition)
		{
			argPosition = 0;

			string content = msg.Content;

			if (string.IsNullOrEmpty(content)) return false;

			string mentionRegex = $"<@!?{Bot.ClientUserId}>";
			Regex mention = new(mentionRegex);
			
			Match? match = mention.Match(content, 0, Math.Min(content.Length, mentionRegex.Length - 1));
			
			if (match.Success) 
			{
				argPosition = match.Value.Length;
				return true;
			}

			if (content.StartsWith(prefix)) 
				argPosition = prefix.Length;
			else if(msg.Channel is IDMChannel)
				argPosition = 0;
			else return false;

			return prefix is not ("~" or "|" or "*" or "_" or "`")
				|| !content.StartsWith(prefix + prefix)
				|| !content.EndsWith(prefix + prefix);
		}

		private static async Task BabResult(CommandInfo method, CommandContext context, IResult result)
		{
			if (await CommandErrorType(method, context, result)) return;

			var exception = ((ExecuteResult)result).Exception;

			switch (exception)
			{
				case HttpException httpException:
					await HandleHttpException(context, httpException);
					break;

				case ReplyException replyException:
					await replyException.Send(context.Channel);
					break;

				default:
					StringBuilder builder = new();

					builder.Append(exception.Message, DiscordDecorationType.Highlight);
					string? stackTrace = exception.StackTrace;

					if (stackTrace == null)
					{
						await context.Channel.SendMessageAsync(builder.ToString());
						return;
					}

					if (context.User.Id == Bot.OwnerId)
					{
						int maxLength = 1800 - builder.Length;
						if (stackTrace.Length > maxLength)
						{
							stackTrace = stackTrace[..maxLength] + 
								$"\n[...{stackTrace.Length - maxLength}chars]";
						}

						builder.Append(stackTrace, DiscordDecorationType.Block);
					}
					else
					{
						int end = stackTrace.IndexOf('\n');

						if (end > -1 && end < 1800)
							builder.Append(stackTrace[..end], DiscordDecorationType.Block);
					}

					await context.Channel.SendMessageAsync(exception.Message);
					break;
			}
		}

		private static async Task HandleHttpException(CommandContext context, HttpException httpException)
		{
			switch (httpException.HttpCode)
			{
				case System.Net.HttpStatusCode.Forbidden:
					{
						await context.PermissionError(context, httpException);
					}
					break;
				case System.Net.HttpStatusCode.BadRequest:
					{
						if (Log != null)
							await Log.DiscordException(httpException);
					}
					break;
				default:
					await context.Channel.SendMessageAsync(httpException.ToString());
					break;
			}
		}

		private static async Task<bool> CommandErrorType(CommandInfo method, CommandContext context, IResult result)
		{
			switch (result.Error)
			{
				case CommandError.Exception: return false;
				case CommandError.UnknownCommand: 
					//await context.Channel.SendMessageAsync("Hmm?");
					return true;
				case CommandError.ParseFailed:
				case CommandError.BadArgCount:
					await context.Channel.SendMessageAsync($"`{result.ErrorReason}` {Environment.NewLine}" +
						$"Type `{context.Prefix}chelp {method.Name}` for more details",
						embed: new CommandInfoEmbed(method, context.Prefix, true).Embed);
				return true;
				default:
					await context.Channel.SendMessageAsync($"Something went wrong. `{result.ErrorReason}`");
					Log?.Log(result.ErrorReason);
					return true;
			}
		}

		private DiscordSocketClient client;
		private CommandService service;

		public CommandHandler(DiscordSocketClient client)
		{
			SetClient(client);

			service = new CommandService(new CommandServiceConfig()
			{
				DefaultRunMode = RunMode.Async,
			});

			SetupModules();

			//service.AddModulesAsync(typeof(IBS_Web.Discord.DiscordModules.Commands.HomebrewBuilder.System.SystemBuilder).Assembly, null);

			service.CommandExecuted += OnCommandExecutedAsync;
		}

		public async void SetupModules()
		{
			var modules = await service.AddModulesAsync(Assembly.GetEntryAssembly(), null);
		}

		public void SetClient(DiscordSocketClient client)
		{
			this.client = client;

			this.client.MessageReceived += HandleCommandAsync;

			_instance = this;
		}

		private async Task HandleCommandAsync(SocketMessage socketMessage)
		{
			if (Bot.CurrentState == Bot.ActiveState.Booting ||
				Bot.CurrentState == Bot.ActiveState.Exiting || 
				socketMessage.Author.IsBot) return;

			if (socketMessage is not SocketUserMessage userMessage) return;

			CommandContext context = Bot.ContextBuilder(client, userMessage);

			if (!context.AcceptCommand) return;

			if (!MessageIsApproved(userMessage, context.Prefix, out int argPosition)) 
				return;

			switch (Bot.CurrentState)
			{
				case Bot.ActiveState.Paused:
					if (socketMessage.Author.Id != Bot.OwnerId)
					{
						await context.Channel.SendMessageAsync(
							"Server under maintenance, please refer to the support server for more information.");
						return;
					}
					break;
				case Bot.ActiveState.Updating:
					await context.Channel.SendMessageAsync("Leaving for tea break soon. [Incoming Update]");
					break;
			}

			if (!RunUser(socketMessage.Author.Id)) return;

			await context.BeforeCommandExecute();

			_ = Task.Run(async () =>
			{
				try
				{
					await service.ExecuteAsync(context, argPosition, null);
				}
				catch (Exception e)
				{
					Log?.Log(e);
				}
			});
		}

		public async Task OnCommandExecutedAsync(Optional<CommandInfo> commandInfo, 
			ICommandContext ctx, IResult result)
		{
			running.Remove(ctx.User.Id);

			if (ctx is not CommandContext context) return;

			if (!commandInfo.IsSpecified) return;

			CommandInfo command = commandInfo.Value;

			try
			{
				if (!result.IsSuccess)
				{
					await BabResult(command, context, result);
					return;
				}

				await context.SuccessfulExecution(this, command);
			}
			catch (Exception e)
			{
				Log?.Log(e);
			}

		}
	}
}

