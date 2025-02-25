using Discord.Bot.Handler;
using Discord.Commands;

namespace Discord.Bot.CoreCommands.HelpCommands
{
	public partial class HelpHandler
	{
		public static CommandService Service => CommandHandler.CommandService;
		//Have modules

		#region Modules
		public static IEnumerable<ModuleInfo> Modules => Service.Modules;
		public static readonly string[] ModuleNames = Modules.Select(m => m.Name).OrderBy(n => n).ToArray();
		#endregion

		//Display modules list

		//Display module, which commands list

		//search for commands
		// module name
		// - - command name

		//Display command
		//Module name + command info
	}
}
