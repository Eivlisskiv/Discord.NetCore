using Discord.Bot.Exceptions;
using Discord.Bot.Handler;
using Discord.Bot.Handlers;
using Discord.Bot.Logger;
using Discord.Net;
using Discord.Rest;
using Discord.Utils.Emotes;
using Discord.Utils.Extensions;
using Discord.WebSocket;
using System.Reflection;
using ILogger = Discord.Bot.Logger.ILogger;

namespace Discord.Bot
{
	public class Bot : IBotEvents
	{
		public static Bot Instance { get; private set; }

		public virtual ILogger Log { get; } = new DiscordLogger();

		public enum ActiveState
		{
			Booting,
			Paused,
			Ready,
			Updating,
			Exiting,
			Exited
		}

		public ulong ClientUserId { get; private set; }

		public ulong OwnerId { get; private set; }

		public ActiveState CurrentState { get; private set; }

		public string DefaultPrefix { get; set; } = "~";

		public DiscordSocketClient Client { get; private set; }

		private CommandHandler commandHandler;

		public Bot()
		{
			Client = new DiscordSocketClient();

			Client.Ready += Ready;
			Client.Log += LogAsync;
			Client.JoinedGuild += OnJoinedGuild;
			Client.LeftGuild += OnLeftGuild;

			Client.ReactionAdded += OnReactionAdded;
			Client.ReactionRemoved += OnReactionRemoved;

			Client.Disconnected += OnDisconnected;

			Instance = this;
		}

		public virtual async Task Start(string token)
		{
			await Client.LoginAsync(TokenType.Bot, token);
			await Client.StartAsync();

			await SetCommandHandler();
		}

		public virtual async Task Ready()
		{
			CurrentState = ActiveState.Ready;

			ClientUserId = Client.CurrentUser.Id;

			RestApplication info = await Client.GetApplicationInfoAsync();
			OwnerId = info.Owner.Id;
		}

		private async Task SetCommandHandler()
		{
			if (commandHandler == null)
			{
				commandHandler = new CommandHandler(Client);
				await SetUpCommandService(commandHandler.service);
			}
			else
			{
				commandHandler.SetClient(Client);
			}
		}
		protected virtual async Task SetUpCommandService(Commands.CommandService service)
		{
			await service.AddModulesAsync(GetType().Assembly, null);
		}

		public virtual CommandContext CreateCommandContext(IDiscordClient client, IUserMessage message) 
			=> new CommandContext(client, message);

		public virtual Task OnDisconnected(Exception arg) 
		{
			return Task.CompletedTask;
		}

		public virtual Task LogAsync(LogMessage message) 
		{
			return Task.CompletedTask;
		}

		public virtual Task OnJoinedGuild(SocketGuild guildJoined) 
		{
			return Task.CompletedTask;
		}

		public virtual Task OnLeftGuild(SocketGuild guildLeft) 
		{
			return Task.CompletedTask;
		}

		public Task OnReactionAdded(Cacheable<IUserMessage, ulong> message,
			Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
		{
			if (reaction.UserId == ClientUserId) return Task.CompletedTask;

			return OnReactionAdded(new ReactionInfo(reaction, message, channel));
		}

		protected virtual Task OnReactionAdded(ReactionInfo reaction)
			=> Task.CompletedTask;

		public virtual Task OnReactionRemoved(Cacheable<IUserMessage, ulong> message,
			Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
		{
			if (reaction.UserId == ClientUserId) return Task.CompletedTask;

			return OnReactionRemoved(new ReactionInfo(reaction, message, channel));
		}

		protected virtual Task OnReactionRemoved(ReactionInfo reaction)
			=> Task.CompletedTask;

		public virtual bool IsMessageCommand(ValidationResult baseValidationResult, out int argPosition)
		{
			return baseValidationResult.Validate(out argPosition);
		}

		public async Task HandleException(Exception exception, IMessageChannel channel, IUser user)
		{
			switch (exception)
			{
				case HttpException httpException:
					await HandleHttpException(httpException, channel);
					break;

				case ReplyException replyException:
					await replyException.Send(channel);
					break;

				default:
					await HandleUnknownException(exception, channel, user);
					break;
			}
		}

		protected virtual async Task HandleUnknownException(Exception exception, IMessageChannel channel, IUser user)
		{
			await channel.SendMessageAsync(exception.Message);

			if (user.Id != OwnerId) return;

			if (exception.StackTrace is not null)
				await channel.SendAsFileAsync(exception.StackTrace);
		}

		private async Task HandleHttpException(HttpException httpException, IMessageChannel channel)
		{
			switch (httpException.HttpCode)
			{
				case System.Net.HttpStatusCode.Forbidden:
						await PermissionError(httpException, channel);
					break;
				case System.Net.HttpStatusCode.BadRequest:
					if (CommandHandler.Log != null)
						await CommandHandler.Log.DiscordException(httpException);
					break;
				default:
					await channel.SendMessageAsync(httpException.ToString());
					break;
			}
		}

		private async Task PermissionError(HttpException httpException, IMessageChannel channel)
		{
			string? requiredPerms = null;

			IGuildUser client = (IGuildUser)Client;

			var chanPerms = client.GetPermissions((IGuildChannel)channel);
			requiredPerms += GetMissingPermissions(chanPerms);

			requiredPerms = requiredPerms == null ? " | Unknown permission missing" :
				" | Required Permissions: " + Environment.NewLine + requiredPerms;

			await channel.SendMessageAsync(httpException.Reason + requiredPerms);
		}

		protected virtual string GetMissingPermissions(ChannelPermissions chanPerms)
		{
			string requiredPerms = null!;
			if (!chanPerms.Has(ChannelPermission.EmbedLinks))
				requiredPerms += "Embed Links" + Environment.NewLine;
			if (!chanPerms.Has(ChannelPermission.AddReactions))
				requiredPerms += "Add Reactions" + Environment.NewLine;
			if (!chanPerms.Has(ChannelPermission.ReadMessageHistory))
				requiredPerms += "Read Message History" + Environment.NewLine;
			if (!chanPerms.Has(ChannelPermission.AttachFiles))
				requiredPerms += "Attach Files" + Environment.NewLine;
			if (!chanPerms.Has(ChannelPermission.UseExternalEmojis))
				requiredPerms += "Use External Emojis" + Environment.NewLine;
			return requiredPerms;
		}
	}
}
