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
		public static ILogger Log => Bot?.Log;

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

		public readonly CommandService service;
		private DiscordSocketClient client;

		public CommandHandler(DiscordSocketClient client)
		{
			SetClient(client);

			service = new CommandService(new CommandServiceConfig()
			{
				DefaultRunMode = RunMode.Async,
			});

			service.CommandExecuted += OnCommandExecutedAsync;
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

			CommandContext context = Bot.CreateCommandContext(client, userMessage);

			if (!context.AcceptCommand) return;

			ValidationResult basicValidation = new(userMessage, context.Prefix);

			if (!Bot.IsMessageCommand(basicValidation, out int argPosition))
			{
				UnregistereddContent(context);
				return;
			}

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

			StartCommand(context, argPosition);
		}

		private async void StartCommand(CommandContext context, int argPosition)
		{
			try
			{
				await service.ExecuteAsync(context, argPosition, null);
			}
			catch (Exception e)
			{
				Log?.Log(e);
			}
		}

		private async void UnregistereddContent(CommandContext context)
		{
			try
			{
				await context.UnknownCommand();
			}
			catch (Exception e)
			{
				Log?.Log(e);
			}
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
					await context.FailedCommandResult(command, result);
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

