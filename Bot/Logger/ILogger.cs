using Discord.Net;
using Discord.Utils.Messages;
using System.Text;

namespace Discord.Bot.Logger
{
	public interface ILogger
	{
		void Log(string message);

		void Log(Exception exception);

		Task DiscordException(HttpException exception);
	}

	public class DiscordLogger : ILogger
	{
		public static StringBuilder ToMessage(Exception exception)
		{
			StringBuilder sb = new StringBuilder()
				.AppendTimestamp(DateTime.UtcNow, Utils.DiscordTimeType.LongDateShortTime)
				.AppendFormat("{0}{1}{0}", '\n', exception.Message);

			if(exception.StackTrace != null)
				sb.AppendFormat("{0}{1}{2}{1}{0}",
					Utils.DiscordDecorationType.Block,
					"\n", exception.StackTrace);

			return sb;
		}

		public virtual async Task DiscordException(HttpException exception)
		{
			string message = ToMessage(exception).ToString();

			Log(message);

			Bot bot = Bot.Instance;
			ulong ownerId = bot.OwnerId;

			IUser owner = await bot.Client.GetUserAsync(ownerId);

			if (owner == null) return;

			var dm = await owner.CreateDMChannelAsync();

			await dm.SendMessageAsync(message);
		}

		public virtual void Log(string message)
		{
			Console.WriteLine(message);
		}

		public virtual void Log(Exception exception)
		{
			string message = ToMessage(exception).ToString();

			Log(message);
		}
	}
}
