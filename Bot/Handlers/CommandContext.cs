using Discord.Bot.Handler;
using Discord.Commands;
using Discord.Net;

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

		public async Task PermissionError(CommandContext context, HttpException httpException)
		{
			string? requiredPerms = null;

			var client = (IGuildUser)await context.Channel.GetUserAsync(Bot.Instance.ClientUserId);

			var chanPerms = client.GetPermissions((IGuildChannel)context.Channel);
			requiredPerms += GetMissingPermissions(chanPerms);

			requiredPerms = requiredPerms == null ? " | Unknown permission missing" :
				" | Required Permissions: " + Environment.NewLine + requiredPerms;

			await context.Channel.SendMessageAsync(httpException.Reason + requiredPerms);
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
	}
}
