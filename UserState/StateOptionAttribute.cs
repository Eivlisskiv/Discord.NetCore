namespace Discord.UserState
{
	[AttributeUsage(AttributeTargets.Method)]
	public class StateOptionAttribute : Attribute
	{
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
