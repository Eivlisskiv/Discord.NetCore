namespace Discord.Utils.ToEmbed
{
	public interface IEmbedConvertible
	{
		EmbedBuilder ToEmbed();
	}

	public static class EmbedConvertibleEx
	{
		public static EmbedBuilder ToEmbed<T>(this T obj)
		{
			if(obj is IEmbedConvertible embedConvertible) return embedConvertible.ToEmbed();

			return DefaultEmbedConversion(obj);
		}

		private static EmbedBuilder DefaultEmbedConversion<T>(T obj)
		{
			EmbedBuilder embed = new EmbedBuilder();

			return embed;
		}
	}
}
