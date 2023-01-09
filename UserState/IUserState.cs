using Discord.Utils.Emotes;
using Discord.WebSocket;

namespace Discord.UserState
{
	public interface IUserState
	{
		ulong UserId { get; }

		ulong ChannelId { get; }

		ulong MessageId { get; }

		Task<bool> Reply(IUserMessage message);

		Task<bool> Reaction(ReactionInfo reaction);

		Task SendStateMessage(IMessageChannel channel);
	}

	public abstract partial class CoreUserState : IUserState
	{
		public ulong UserId { get; init; }

		public ulong ChannelId { get; private set; }

		public ulong MessageId { get; private set; }

		private IUser? user;
		private IMessageChannel? channel;
		private IUserMessage? message;

		public CoreUserState()
		{
		}

		public CoreUserState(IUser user)
		{
			this.user = user;
			UserId = user.Id;
		}

		public async Task<bool> Reply(IUserMessage message)
		{
			if (message.Author.Id != UserId ||
				message.Channel.Id != ChannelId)
				return false;

			user = message.Author;
			channel = message.Channel;

			return await UserStateMethods.Invoke(this, message.Content);
		}

		public async Task<bool> Reaction(ReactionInfo reaction)
		{
			if (reaction.UserId != UserId ||
				reaction.ChannelId != ChannelId ||
				reaction.MessageId != MessageId) 
				return false;

			user ??= reaction.CachedUser;
			channel ??= reaction.CachedChannel;
			message ??= reaction.CachedMessage;

			string? key = reaction.Emote?.ToString();

			if (string.IsNullOrEmpty(key)) return false;

			return await UserStateMethods.Invoke(this, key);
		}

		public async Task SendStateMessage(IMessageChannel? channel = null)
		{
			if (channel is null) channel = await GetChannel();
			else SetChannel(channel);

			if (channel is null) return;

			EmbedBuilder embed = await GetEmbed();

			string? text = await GetMessageText();

			message = await channel.SendMessageAsync(text, embed: embed.Build());
			MessageId = message.Id;

			IEnumerable<IEmote> reactions = await UserStateMethods.GetEmotes(this);

			if (reactions.Any()) await message.AddReactionsAsync(reactions);

			await AfterMessageSent();
		}

		protected async Task UpdateOrSendStateMessage(IMessageChannel? channel = null)
		{
			if (this.message is null && MessageId == default)
			{
				await SendStateMessage(channel);
				return;
			}

			if (channel is null) channel = await GetChannel();
			else SetChannel(channel);

			if (channel is null) return;

			EmbedBuilder embed = await GetEmbed();

			string? text = await GetMessageText();

			IUserMessage? message = await GetMessage();

			if (message is null) return;

			await message.ModifyAsync(mp =>
			{
				mp.Embed = embed.Build();
			});

			IEnumerable<IEmote> reactions = await UserStateMethods.GetEmotes(this);

			if (reactions.Any()) await message.AddReactionsAsync(reactions);

			await AfterMessageUpdate();
		}

		protected virtual Task AfterMessageSent()
			=> Task.CompletedTask;

		protected virtual Task AfterMessageUpdate()
			=> Task.CompletedTask;

		protected void SetChannel(IMessageChannel channel)
		{
			this.channel = channel;
			ChannelId = channel.Id;
		}

		protected async Task<IUser?> GetUser()
		{
			if (user == null)
				user = await Bot.Bot.Instance.Client.GetUserAsync(UserId);

			return user;
		}

		protected async Task<IMessageChannel?> GetChannel()
		{
			if (channel == null) 
				channel = await Bot.Bot.Instance.Client.GetChannelAsync(ChannelId) as IMessageChannel;

			return channel;
		}
	
		protected async Task<IUserMessage?> GetMessage()
		{
			if (message == null)
			{
				IMessageChannel? channel = await GetChannel();

				if(channel is not null)
					message = await channel.GetMessageAsync(MessageId) as IUserMessage;
			}

			return message;
		}

		protected async Task LoadDiscordEntities()
		{
			await GetUser();
			await GetChannel();
			await GetMessage();
		}

		protected virtual Task<EmbedBuilder> GetEmbed()
		{
			EmbedBuilder builder = new EmbedBuilder();

			return Task.FromResult(builder);
		}

		protected virtual Task<string?> GetMessageText()
		{
			return Task.FromResult<string?>(null);
		}
	}
}
