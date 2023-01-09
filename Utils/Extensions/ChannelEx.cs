namespace Discord.Utils.Extensions
{
	public static class ChannelEx
	{
		public static async Task SendAsFileAsync(this IMessageChannel channel,
			string fileContent,
			string? text = null,
			bool isTTS = false,
			Embed? embed = null,
			RequestOptions? options = null,
			bool isSpoiler = false,
			AllowedMentions? allowedMentions = null,
			MessageReference? messageReference = null,
			MessageComponent? components = null,
			ISticker[]? stickers = null,
			Embed[]? embeds = null,
			MessageFlags flags = MessageFlags.None)
		{
			string filePath = $"./_temp_{Guid.NewGuid()}.txt";

			try
			{
				using (StreamWriter writer = new(filePath))
				{
					writer.Write(fileContent);
				}

				await channel.SendFileAsync(filePath, text, isTTS, embed,
					options, isSpoiler, allowedMentions, messageReference,
					components, stickers, embeds, flags);
			}
			finally
			{
				if(File.Exists(filePath)) File.Delete(filePath);
			}
		}
	}
}
