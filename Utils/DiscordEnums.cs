namespace Discord.Utils
{
	public static class DiscordEnums
	{
		public static string ToTag(this DiscordDecorationType type)
			=> type switch
			{
				DiscordDecorationType.Bold => "**",
				DiscordDecorationType.Underline => "__",
				DiscordDecorationType.Italic => "*",
				DiscordDecorationType.Highlight => "`",
				DiscordDecorationType.Block => "```",
				DiscordDecorationType.Strikethrough => "~~",
				DiscordDecorationType.Spoiler => "||",
				_ => "",
			};

		public static string MentionFormat(this DiscordMentionsType type)
			=> type switch
			{
				DiscordMentionsType.User => "<@{0}>",
				DiscordMentionsType.Role => "<@&{0}>",
				DiscordMentionsType.Channel => "<#{0}>",
				DiscordMentionsType.Emote => "<{1}:{0}>",
				DiscordMentionsType.AnimatedEmote => "<a:{1}:{0}>",
				_ => "{1} ({0})"
			};
	}

	public enum DiscordDecorationType
	{
		Bold,
		Underline,
		Italic,
		Highlight,
		Strikethrough,
		Block,
		Spoiler,
	}

	public enum DiscordTimeType
	{
		ShortTime = 't',
		LongTime = 'T',
		ShortDate = 'd',
		LongDate = 'D',
		LongDateShortTime = 'f',
		LongDateWeekdayLongTime = 'F',
		Relative = 'R'
	}

	public enum DiscordMentionsType
	{
		User,
		Role,
		Channel,
		Emote,
		AnimatedEmote,
	}
}
