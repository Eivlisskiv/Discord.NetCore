using Discord.WebSocket;

namespace Discord.Utils.Emotes
{
	public class ReactionInfo
	{
		private static DiscordSocketClient Client => Bot.Bot.Instance.Client;

		public ulong UserId => socketReaction.UserId;
		public ulong MessageId => socketReaction.MessageId;
		public ulong ChannelId => cacheableChannel.Id;
		public IEmote Emote => socketReaction.Emote;

		public readonly SocketReaction socketReaction;
		public readonly Cacheable<IUserMessage, ulong> cacheableMessage;
		public readonly Cacheable<IMessageChannel, ulong> cacheableChannel;

		public IUserMessage? CachedMessage { get; private set; }
		public IMessageChannel? CachedChannel { get; private set; }
		public IUser? CachedUser { get; private set; }

		public ReactionInfo(SocketReaction socketReaction, 
			Cacheable<IUserMessage, ulong> message,
			Cacheable<IMessageChannel, ulong> channel)
		{
			this.socketReaction = socketReaction;
			cacheableMessage = message;
			cacheableChannel = channel;

			CachedChannel = GetCachedChannel();
			CachedMessage = GetCachedMessage();
			CachedUser = GetCachedUser();
		}

		private IMessageChannel GetCachedChannel()
		{
			if (CachedMessage is not null)
				return CachedMessage.Channel;

			if (socketReaction.Channel is not null)
				return socketReaction.Channel;

			if (cacheableChannel.HasValue && cacheableMessage.Value is not null)
				return cacheableChannel.Value;

			if (cacheableMessage.HasValue && cacheableMessage.Value?.Channel is not null)
				return cacheableMessage.Value.Channel;

			return null;
		}

		public async Task<IMessageChannel> GetChannel()
		{
			if (CachedChannel is not null) return CachedChannel;

			return (CachedChannel = await Client.GetChannelAsync(ChannelId) as IMessageChannel)!;
		}

		private IUserMessage? GetCachedMessage()
		{
			if (socketReaction.Message.IsSpecified && socketReaction.Message.Value is not null)
				return socketReaction.Message.Value;

			if (cacheableMessage.HasValue && cacheableMessage.Value is not null)
				return cacheableMessage.Value;

			return null;
		}

		public async Task<IUserMessage> GetMessage()
		{
			if (CachedMessage is not null) return CachedMessage;

			IMessageChannel channel = await GetChannel();

			return (CachedMessage = await channel.GetMessageAsync(MessageId) as IUserMessage)!;
		}

		private IUser? GetCachedUser()
		{
			if (socketReaction.User.IsSpecified && socketReaction.User.Value is not null)
				return socketReaction.User.Value;

			if (CachedMessage is not null && UserId == CachedMessage.Author.Id)
				return CachedMessage.Author;

			return null;
		}

		public async Task<IUser> GetUser()
		{
			if (CachedUser is not null) return CachedUser;

			return (CachedUser = await Client.GetUserAsync(UserId))!;
		}
	}
}
