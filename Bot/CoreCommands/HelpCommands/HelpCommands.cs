using Discord.Bot.Handlers;
using Discord.Commands;
using Utilities.Extensions;
using static Discord.Bot.Handlers.DiscordEx;
using CommandContext = Discord.Bot.Handlers.CommandContext;

namespace Discord.Bot.CoreCommands.HelpCommands
{
	public class HelpCommands : ModuleBase<CommandContext>
	{
		[Command("Modules")]
		[Alias("Module", "Groups")]
		public async Task Help2(string moduleName = null, [Remainder] string commandName = null)
		{
			if (moduleName == null)
			{
				string[] modulelist = HelpHandler.ModuleNames;

				string message = "Please enter command category";
				EmbedBuilder embed = new EmbedBuilder()
					.WithTitle("Command Categories")
					.WithDescription(string.Join(Environment.NewLine, modulelist))
					.WithColor(Context.BotColor)
					.AddField("Example", 
					$"{Context.Prefix}module {modulelist.RandomElement()}");

				await ReplyAsync(message, embed:embed.Build());
				return;
			}

			ModuleInfo module = HelpHandler.Modules.First(
				m => m.Name.Equals(moduleName, StringComparison.OrdinalIgnoreCase));

			if (module == null)
			{
				await ReplyAsync("Module was not found");
				return;
			}

			if (module.Commands.Count == 0)
			{
				await ReplyAsync($"Module {module.Name} is empty");
				return;
			}

			if (commandName == null)
			{
				await ReplyAsync("Please enter command name", embed:

					EmbedEx.BuildEmbed("Commands", string.Join(Environment.NewLine, module.Commands.Select(c => c.Name)), null, Color.DarkRed,
				EmbedEx.NewField("Example", $"{Context.Prefix}modules {module.Name} " +
					$"{module.Commands.RandomElement()?.Name}")).Build());
				return;
			}
			
			CommandInfo ci = module.Commands.First(c => c.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));
			if (ci == null) await ReplyAsync($"Command {commandName} not found");
			else await ReplyAsync(embed: new CommandInfoEmbed(
				ci, Context.Prefix, color: Context.BotColor).Embed
				);
			
			
		}

		[Command("Help")]
		public async Task CommandHelp([Remainder] string commandName)
		{
			if (commandName.Length == 0) await ReplyAsync("No command name was given.");
			else
			{
				var search = HelpHandler.Service.Search(commandName);
				if (!search.IsSuccess) await ReplyAsync(search.ErrorReason);
				else
				{
					CommandInfo ci = search.Commands[0].Command;
					await ReplyAsync(embed: new CommandInfoEmbed(ci, Context.Prefix, color:Context.BotColor).Embed);
				}
			}
		}
	}
}
