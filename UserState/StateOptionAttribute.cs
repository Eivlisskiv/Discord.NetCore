namespace Discord.UserState
{
	[AttributeUsage(AttributeTargets.Method)]
	public class StateOptionAttribute : Attribute
	{
		public char EmoteCharacter
		{
			get => emote[0];
			set => emote = value.ToString();
		}

		public string emote;

		public string[] aliases;

		public StateOptionAttribute(params string[] aliases)
		{
			this.aliases = aliases;
		}
	}

	public class StateOptionVerificationAttribute : StateOptionAttribute
	{

	}
}
