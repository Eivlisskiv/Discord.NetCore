using Discord.Net;
using System;
using System.Threading.Tasks;

namespace Discord.Bot.Logger
{
	public interface ILogger
	{
		void LogS(string message);

		void LogS(Exception exception);
		Task DiscordException(HttpException exception);
	}
}
