using Discord.Bot.Handler;
using Discord.Commands;
using Discord.Net;
using System;
using System.Threading.Tasks;

namespace Discord.Bot.Handlers
{
	public class CommandContext : Discord.Commands.CommandContext
	{
		public virtual bool AcceptCommand => true;

		public virtual string Prefix => Bot.Instance.DefaultPrefix;

		public CommandContext(IDiscordClient client, IUserMessage msg) 
			: base(client, msg) { }

		public virtual void BeforeCommandExecute()
		{

		}

		public virtual void SuccessfulExecution(CommandHandler commandHandler, CommandInfo command) 
		{
			
		}

		public async Task PermissionError(CommandContext context, HttpException httpException)
		{
			string requiredPerms = null;

			var client = (IGuildUser)await context.Channel.GetUserAsync(Bot.Instance.ClientUserId);

			var chanPerms = client.GetPermissions((IGuildChannel)context.Channel);
			requiredPerms = GetMissingPermissions(requiredPerms, chanPerms);

			requiredPerms = requiredPerms == null ? " | Unknown permission missing" :
				" | Required Permissions: " + Environment.NewLine + requiredPerms;

			await context.Channel.SendMessageAsync(httpException.Reason + requiredPerms);
		}

		protected virtual string GetMissingPermissions(string requiredPerms, ChannelPermissions chanPerms)
		{
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
