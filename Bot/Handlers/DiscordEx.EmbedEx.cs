using System.Collections.Generic;

namespace Discord.Bot.Handlers
{
	public partial class DiscordEx
	{
		public static class EmbedEx
		{
			public static EmbedBuilder BuildEmbed(string title, string? desc = null, string? footer = null,
				Color color = default, params EmbedFieldBuilder[] fields)
			{
				EmbedBuilder embed = new();
				embed.WithTitle(title);

				if (desc != null && desc.Length > 0)
					embed.WithDescription(desc);

				embed.WithColor(color);

				if (footer != null && footer.Length > 0)
					embed.WithFooter(footer);

				embed.WithFields(fields);

				return embed;
			}

			public static EmbedBuilder BuildEmbed(string title, string? desc = null, string? footer = null,
				Color color = default, IEnumerable<EmbedField>? fields = null)
			{
				EmbedBuilder embed = new();
				embed.WithTitle(title);

				if (desc != null && desc.Length > 0)
					embed.WithDescription(desc);

				embed.WithColor(color);

				if (footer != null && footer.Length > 0)
					embed.WithFooter(footer);

				if (fields != null)
					foreach (var f in fields)
						embed.AddField(NewField(f.Name, f.Value, f.Inline));

				return embed;
			}

			public static EmbedFieldBuilder NewField(string title, string content, bool inline = false) => new EmbedFieldBuilder()
			{
				Name = title,
				Value = content,
				IsInline = inline,
			};
		}
	}
}
