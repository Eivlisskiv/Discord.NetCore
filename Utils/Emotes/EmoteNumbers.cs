using System.Text;

namespace Discord.Utils.Emotes
{
	public static partial class Emotes 
	{
		public const char Negative = '➖';

		private const string NumberFormat = "\\u003{0}\\u20e3";

		private static string CharacterNumberEmote(char c)
		{
			return string.Format(NumberFormat, c);
		}

		public static string ToEmoteValue(this int digit)
		{
			if (digit < 0 || digit > 9) throw new ArgumentOutOfRangeException(nameof(digit));

			return string.Format(NumberFormat, digit);
		}

		public static IEmote ToEmote(this int digit)
		{
			string value = digit.ToEmoteValue();

			return new Emoji(value);
		}

		public static string ToEmoteValues(this int number, string seperator = "")
		{
			StringBuilder builder = new();

			string str = number.ToString();
			for (int i = 0; i < str.Length; i++)
			{
				char character = str[i];

				if (character is '-')
				{
					builder.Append(Negative);
					continue;
				}

				builder.AppendFormat(NumberFormat, character);

				if (!string.IsNullOrEmpty(seperator) && i < str.Length - 1)
					builder.Append(seperator);
			}

			return builder.ToString();
		}

		public static string[] ToEmoteValues(this int number)
		{
			string str = number.ToString();
			string[] values = new string[str.Length];

			for (int i = 0; i < str.Length; i++)
			{
				char c = str[i];
				values[i] = c switch
				{
					'-' => Negative.ToString(),
					_ => CharacterNumberEmote(c),
				};
			}

			return values;
		}

		public static IEmote[] ToEmotes(this int number)
		{
			string str = number.ToString();
			IEmote[] values = new IEmote[str.Length];

			for (int i = 0; i < str.Length; i++)
			{
				char c = str[i];

				string emoji = c switch
				{
					'-' => Negative.ToString(),
					_ => CharacterNumberEmote(c),
				};

				values[i] = new Emoji(emoji);
			}

			return values;
		}
	}
}
