using Discord.WebSocket;
using System.Text.RegularExpressions;

namespace Discord.Bot.Handler
{
	public struct ValidationResult
	{
		public int ArgPosition { get; private set; }

		public string Prefix { get; }

		public SocketUserMessage Message { get; }

		public bool IsSuccess { get; private set; }

		public ValidationResult(SocketUserMessage msg, string prefix)
		{
			ArgPosition = 0;
			IsSuccess = false;

			Message = msg;
			Prefix = prefix;
		}

		public bool Validate(out int position)
		{
			string content = Message.Content;

			if (string.IsNullOrEmpty(content)) return Failure(out position);

			string mentionRegex = $"<@!?{Bot.Instance.ClientUserId}>";
			Regex mention = new(mentionRegex);

			Match? match = mention.Match(content, 0, Math.Min(content.Length, mentionRegex.Length - 1));

			if (match.Success)
			{
				position = SetArgPosition(match.Value.Length);
				return Success();
			}

			if (content.StartsWith(Prefix))
				position = SetArgPosition(Prefix.Length);
			else if (Message.Channel is IDMChannel)
				position = SetArgPosition(0);
			else return Failure(out position);

			IsSuccess = Prefix is not ("~" or "|" or "*" or "_" or "`")
				|| !content.StartsWith(Prefix + Prefix)
				|| !content.EndsWith(Prefix + Prefix);

			return IsSuccess;
		}

		private int SetArgPosition(int position)
		{
			return ArgPosition = position;
		}

		private bool Failure(out int pos)
		{
			ArgPosition = pos = 0;
			return IsSuccess = false;
		}

		private bool Success()
		{
			return IsSuccess = true;
		}
	}
}