using Discord;

namespace IBS_Web.Discord.Discord.NetCore.Utils.ToEmbed
{
	public interface IEmbedFieldConvertible
	{
		EmbedFieldBuilder ToEmbedField();
	}

	public static class EmbedFieldConvertibleEx
	{
		public static EmbedFieldBuilder ToEmbedField<T>(this T obj)
		{
			if (obj is IEmbedFieldConvertible embedConvertible) return embedConvertible.ToEmbedField();

			return DefaultConversion(obj);
		}

		private static EmbedFieldBuilder DefaultConversion<T>(T obj)
		{
			EmbedFieldBuilder field = new EmbedFieldBuilder();

			return field;
		}
	}
}
