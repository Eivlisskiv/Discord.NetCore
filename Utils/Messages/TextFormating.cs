using System.Text;

namespace Discord.Utils.Messages
{
	public static class TextFormating
	{
		public static StringBuilder Append(this StringBuilder sb, object value, DiscordDecorationType type)
			=> sb.AppendFormat("{0}{1}{0}", new object[] { type.ToTag(), value });

		public static StringBuilder Format(this StringBuilder sb, DiscordDecorationType type)
			=> sb.Wrap(type.ToTag());

		public static StringBuilder Wrap(this StringBuilder sb, string wrap)
			=> sb.Insert(0, wrap).Insert(sb.Length - 1, wrap);

		public static StringBuilder AppendTimestamp(this StringBuilder sb, DateTime time, DiscordTimeType type = DiscordTimeType.LongDateShortTime)
			=> sb.AppendFormat("<t:{0}:{1}>", new object[] { ((DateTimeOffset)time).ToUnixTimeSeconds(), (char)type });

		public static StringBuilder AppendObjectTag(this StringBuilder sb, 
			DiscordMentionsType type, ulong id, string?	name = null)
			=> sb.AppendFormat(type.MentionFormat(), new object[] { id, name! });

		public static StringBuilder AppendLink(this StringBuilder sb,
			string title, string url)
			=> sb.AppendFormat("[{0}]({1})", new { title, url });

		public static StringBuilder AppendProtocolLink(this StringBuilder sb,
			string target, string command = ".")
			=> sb.AppendFormat("<{0}://{1}>", new { target, command });

		public static StringBuilder QuotedAppend(this StringBuilder sb,
			object value, bool isMultiline = false)
			=> sb.AppendFormat("{1} {0}", new object[] { value, isMultiline ? ">>>" : '>' });

		public static StringBuilder AppendMention(this StringBuilder sb, object value)
		{
			return value switch
			{
				IUser user => sb.AppendFormat(DiscordMentionsType.User.MentionFormat(), user.Id),
				IRole role => sb.AppendFormat(DiscordMentionsType.Role.MentionFormat(), role.Id),
				IChannel channel => sb.AppendFormat(DiscordMentionsType.Channel.MentionFormat(), channel.Id),

				Emote emote => sb.AppendFormat(
					(emote.Animated 
					? DiscordMentionsType.Emote 
					: DiscordMentionsType.AnimatedEmote
					).MentionFormat(),
					emote.Id, emote.Name),

				_ => sb.Append(value)
			};
		}
	}
}
