using Discord.Bot.Exceptions;
using Discord.Bot.Handler;
using Discord.Commands;
using Discord.Net;
using Discord.Utils;
using Discord.Utils.Extensions;
using Discord.Utils.Messages;
using System.Text;

namespace Discord.Bot.Handlers
{
	public class CommandContext : Commands.CommandContext
	{
		public virtual bool AcceptCommand => true;

		public virtual string Prefix => Bot.Instance.DefaultPrefix;

		public IGuildUser? BotGuildUser { get; private set; }

		public Color BotColor { get; private set; }

		protected virtual Color DefaultBotColor => Color.DarkBlue;

		public CommandContext(IDiscordClient client, IUserMessage msg) 
			: base(client, msg) { }

		public virtual async Task BeforeCommandExecute()
		{
			BotColor = DefaultBotColor;
			await GetBotColor();
		}

		private async Task GetBotColor()
		{
			if (Guild == null) return;

			BotGuildUser = await Guild.GetUserAsync(Bot.Instance.ClientUserId);
			if (BotGuildUser is null) return;

			foreach (ulong id in BotGuildUser.RoleIds)
			{
				IRole? role = Guild.GetRole(id);

				if (role.IsManaged) continue;

				BotColor = role.Color;
				break;
			}
		}

		public virtual Task SuccessfulExecution(CommandHandler commandHandler, CommandInfo command) 
		{
			return Task.CompletedTask;
		}

		public virtual async Task FailedCommandResult(CommandInfo method, Commands.IResult result)
		{
			if (await CommandErrorType(method, result)) return;

			Exception? exception = ((ExecuteResult)result).Exception;

			if (exception is not null)
				await Bot.Instance.HandleException(exception, Channel, User);
		}

		private async Task<bool> CommandErrorType(CommandInfo method, Commands.IResult result)
		{
			switch (result.Error)
			{
				case CommandError.Exception: return false;
				case CommandError.UnknownCommand:
					//await context.Channel.SendMessageAsync("Hmm?");
					return true;
				case CommandError.ParseFailed:
				case CommandError.BadArgCount:
					await Channel.SendMessageAsync($"`{result.ErrorReason}` {Environment.NewLine}" +
						$"Type `{Prefix}chelp {method.Name}` for more details",
						embed: new CommandInfoEmbed(method, Prefix, true).Embed);
					return true;
				default:
					await Channel.SendMessageAsync($"Something went wrong. `{result.ErrorReason}`");
					CommandHandler.Log?.Log(result.ErrorReason);
					return true;
			}
		}
	}
}
