using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections;
using Discord.Bot.Logger;
using CommandContext = Discord.Bot.Handlers.CommandContext;
using Discord.Bot.Handlers;

namespace Discord.Bot.Handler
{
	public class CommandHandler
	{
		public static Bot Bot => Bot.Instance;
		public static ILogger Log;

		public static readonly Hashtable running = new();

		private static CommandHandler _instance;
		internal static CommandService CommandService => _instance._command;

		public static bool RunUser(ulong id)
		{
			if (!running.ContainsKey(id))
			{
				running.Add(id, DateTime.UtcNow);
				return true;
			}
			else if (((DateTime)running[id]).AddMinutes(2) < DateTime.UtcNow)
			{
				running[id] = DateTime.UtcNow;
				return true;
			}

			Log.LogS($"Waiting for command execution to end for {id}");
			return false;
		}

		private static async Task BabResult(CommandInfo method, CommandContext context, IResult result)
		{
			if (await CommandErrorType(method, context, result)) return;

			var exception = ((ExecuteResult)result).Exception;

			if (exception is Discord.Net.HttpException httpException)
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
						await Log.DiscordException(httpException);
					}
					break;
					default:
						await context.Channel.SendMessageAsync(httpException.ToString());
						break;
				}

				return;
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
					Log.LogS(result.ErrorReason);
					return true;
			}
		}

		private DiscordSocketClient _client;
		private CommandService _command;

		public CommandHandler(DiscordSocketClient client)
		{
			SetClient(client);

			_command = new CommandService();

			_command.AddModulesAsync(Assembly.GetEntryAssembly(), null);

			_command.CommandExecuted += OnCommandExecutedAsync;

		}

		public void SetClient(DiscordSocketClient client)
		{
			_client = client;

			_client.MessageReceived += HandleCommandAsync;

			_instance = this;
		}

		private bool MessageIsApproved(SocketUserMessage msg, string prefix, out int argPosition)
		{
			argPosition = 0;
			if (msg.HasStringPrefix($"<@!{_client.CurrentUser.Id}> ", ref argPosition))
				return true;

			if (msg.Channel is not IGuildChannel gc && msg.HasStringPrefix(Bot.DefaultPrefix, ref argPosition))
				return true;

			switch (prefix)
			{
				case "~":
				case "|":
				case "*":
				case "_":
				case "`":
				{
					string content = msg.Content;
					if (content.StartsWith(prefix + prefix) && content.EndsWith(prefix + prefix))
						return false;
					else if (msg.HasStringPrefix(prefix, ref argPosition))
						return true;
				}
				break;
				default:
					if (msg.HasStringPrefix(prefix, ref argPosition))
						return true;
					break;
			}

			return false;
		}

		private async Task HandleCommandAsync(SocketMessage s)
		{
			if (Bot.CurrentState == Bot.ActiveState.Booting ||
				Bot.CurrentState == Bot.ActiveState.Exiting || 
				s.Author.IsBot) return;

			if (s is not SocketUserMessage msg) return;

			CommandContext context = new(_client, msg);

			if (!context.AcceptCommand) return;

			if (!MessageIsApproved(msg, context.Prefix, out int argPosition)) return;

			switch (Bot.CurrentState)
			{
				case Bot.ActiveState.Paused:
					if (s.Author.Id != Bot.OwnerId)
					{
						await context.Channel.SendMessageAsync(
							"Server under maintenance, please refer to the support server for more information.");
						return;
					}
					break;
				case Bot.ActiveState.Ready: break;
				case Bot.ActiveState.Updating:
					await context.Channel.SendMessageAsync("Leaving for tea break soon. [Incoming Update]");
					break;
			}

			if (!RunUser(s.Author.Id)) return;

			context.BeforeCommandExecute();

			_ = Task.Run(async () =>
			{
				try
				{
					await _command.ExecuteAsync(context, argPosition, null);
				}
				catch (Exception e)
				{
					Log.LogS(e);
				}
			});
		}

		public async Task OnCommandExecutedAsync(Optional<CommandInfo> commandInfo, 
			ICommandContext ctx, IResult result)
		{
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

				context.SuccessfulExecution(this, command);
			}
			catch (Exception e)
			{
				Log.LogS(e);
			}

			running.Remove(ctx.User.Id);
		}
	}
}

