using Discord.Bot.Exceptions;
using Discord.Bot.Handler;
using Discord.Commands;
using Discord.Net;
using Discord.Utils;
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

			var exception = ((ExecuteResult)result).Exception;

			switch (exception)
			{
				case HttpException httpException:
					await HandleHttpException(httpException);
					break;

				case ReplyException replyException:
					await replyException.Send(Channel);
					break;

				default:
					StringBuilder builder = new();

					builder.Append(exception.Message, DiscordDecorationType.Highlight);
					string? stackTrace = exception.StackTrace;

					if (stackTrace == null)
					{
						await Channel.SendMessageAsync(builder.ToString());
						return;
					}

					if (User.Id == Bot.Instance.OwnerId)
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

					await Channel.SendMessageAsync(exception.Message);
					break;
			}
		}

		public async Task PermissionError(HttpException httpException)
		{
			string? requiredPerms = null;

			var client = (IGuildUser)await Channel.GetUserAsync(Bot.Instance.ClientUserId);

			var chanPerms = client.GetPermissions((IGuildChannel)Channel);
			requiredPerms += GetMissingPermissions(chanPerms);

			requiredPerms = requiredPerms == null ? " | Unknown permission missing" :
				" | Required Permissions: " + Environment.NewLine + requiredPerms;

			await Channel.SendMessageAsync(httpException.Reason + requiredPerms);
		}

		protected virtual string GetMissingPermissions(ChannelPermissions chanPerms)
		{
			string requiredPerms = null!;
			if (!chanPerms.Has(ChannelPermission.EmbedLinks))
				requiredPerms += "Embed Links" + Environment.NewLine;
			if (!chanPerms.Has(ChannelPermission.AddReactions))
				requiredPerms += "Add Reactions" + Environment.NewLine;
			if (!chanPerms.Has(ChannelPermission.ReadMessageHistory))
				requiredPerms += "Read Message History" + Environment.NewLine;
			if (!chanPerms.Has(ChannelPermission.AttachFiles))
				requiredPerms += "Attach Files" + Environment.NewLine;
			if (!chanPerms.Has(ChannelPermission.UseExternalEmojis))
				requiredPerms += "Use External Emojis" + Environment.NewLine;
			return requiredPerms;
		}

		private async Task HandleHttpException(HttpException httpException)
		{
			switch (httpException.HttpCode)
			{
				case System.Net.HttpStatusCode.Forbidden:
					{
						await PermissionError(httpException);
					}
					break;
				case System.Net.HttpStatusCode.BadRequest:
					{
						if (CommandHandler.Log != null)
							await CommandHandler.Log.DiscordException(httpException);
					}
					break;
				default:
					await Channel.SendMessageAsync(httpException.ToString());
					break;
			}
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
