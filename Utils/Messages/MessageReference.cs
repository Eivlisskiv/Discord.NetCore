namespace Discord.Utils.Messages
{
	public struct MessageReference
	{
		public ulong GuildId { get; set; }

		public ulong ChannelId { get; set; }

		public ulong MessageId { get; set; }
	}
}
