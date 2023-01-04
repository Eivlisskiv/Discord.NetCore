using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Discord.Bot
{
	public interface IBotEvents
	{
		Task LogAsync(LogMessage message);

		Task Ready();

		Task OnJoinedGuild(SocketGuild guildJoined);
		Task OnLeftGuild(SocketGuild guildLeft);

		Task OnReactionAdded(Cacheable<IUserMessage, ulong> message,
			Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction);

		Task OnReactionRemoved(Cacheable<IUserMessage, ulong> message,
			Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction);

		Task OnDisconnected(Exception exception);

	}
}