namespace Discord.Bot.Exceptions
{
	public class ReplyException : Exception
	{
		private string ReplyMessage { get; init; }
		private bool IsTTS { get; init; }
		private Embed Embed { get; init; }
		private RequestOptions Options { get; init; }
		private AllowedMentions AllowedMentions { get; init; }
		private MessageReference MessageReference { get; init; }
		private MessageComponent Components { get; init; }
		private ISticker[] Stickers { get; init; }
		private Embed[] Embeds { get; init; }
		private MessageFlags MessageFlags { get; init; }

		public ReplyException(string replyMessage) : base(replyMessage)
		{
			ReplyMessage = replyMessage;
		}

		public async Task Send(IMessageChannel channel)
		{
			await channel.SendMessageAsync(ReplyMessage, IsTTS, Embed,
				Options, AllowedMentions, MessageReference, Components,
				Stickers, Embeds, MessageFlags);
		}
	}
}
