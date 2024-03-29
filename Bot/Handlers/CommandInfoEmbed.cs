﻿using Discord.Commands;
using EmbedEx = Discord.Bot.Handlers.DiscordEx.EmbedEx;

namespace Discord.Bot.Handlers
{
	class CommandInfoEmbed
	{
		readonly CommandInfo command;

		public Embed Embed => embed.Build();

		private readonly EmbedBuilder embed;

		public CommandInfoEmbed(CommandInfo ci, string prefix, bool isShort = false, Color? color = null)
		{
			command = ci;

			embed = EmbedEx.BuildEmbed(command.Name + " Command Help",
				$"**Syntax:** `{prefix}{command.Name} {(command.Parameters.Count > 0 ? $"{{{string.Join("} {", command.Parameters.Select(p => p.Name))}}}" : null)}`" + Environment.NewLine +
				command.Summary, "use the Help command for more help", color ?? Color.Default);

			if (!isShort)
			{
				if (command.Parameters.Count > 0)
					embed.AddField(EmbedEx.NewField("Arguments",
						string.Join(Environment.NewLine, 
						Enumerable.ToList(command.Parameters).Select(ParamToField))
						, true));

				embed.AddField(EmbedEx.NewField("Aliases", string.Join(Environment.NewLine, command.Aliases), true));
			}
		}

		private string ParamToField(ParameterInfo param)
		{
			return $"{{{param.Name}}} `{GetTypeName(param)}`" + Environment.NewLine +
				$"`Optional? {(param.IsOptional ? $"Yes - Default: {param.DefaultValue}" : "No")}`" + Environment.NewLine +
				param.Summary;
		}

		private string GetTypeName(ParameterInfo param, bool arrayCheck = true)
		{

			switch (param.Name.ToLower())
			{
				case "indexxamount":
				case "slotxamount":
				case "inventoryslotxamount":

					return param.IsMultiple ? "{Slot1}x{Amount} {Slot2}x{Amount} {Slot3}x{Amount} ..." : "{Slot}x{Amount}";
			}

			if (arrayCheck && param.IsMultiple)
			{
				string type = GetTypeName(param, false);
				return $"{type}1 {type}2 {type}3 ...";
			}

			if (param.Type == typeof(IUser)) return "@User";

			if (param.Type == typeof(IMessageChannel)) return "#Channel";

			if (param.Type == typeof(int) || param.Type == typeof(long) || param.Type == typeof(double)) return "Number";

			return "Text";
		}
	}
}

