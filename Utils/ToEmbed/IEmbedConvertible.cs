namespace Discord.Utils.ToEmbed
{
	public interface IEmbedConvertible
	{
		EmbedBuilder ToEmbed(EmbedBuilder? embed);
	}

	public static class EmbedConvertibleEx
	{
		public static EmbedBuilder ToEmbed<T>(this T obj, EmbedBuilder? embed = null)
		{
			if(obj is IEmbedConvertible embedConvertible) 
				return embedConvertible.ToEmbed(embed ?? new());

			return DefaultEmbedConversion(obj, embed ?? new());
		}

		private static EmbedBuilder DefaultEmbedConversion<T>(T obj, EmbedBuilder embed)
		{
			return embed;
		}
	}
}
