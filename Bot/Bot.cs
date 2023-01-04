using Discord.Bot.Handler;
using Discord.Bot.Handlers;
using Discord.Bot.Logger;
using Discord.Rest;
using Discord.WebSocket;
using ILogger = Discord.Bot.Logger.ILogger;

namespace Discord.Bot
{
	public class Bot : IBotEvents
	{
		public static Bot Instance { get; private set; }

		public static Func<IDiscordClient, IUserMessage, CommandContext> ContextBuilder
		{
			get => _contextBuilder;
			set => _contextBuilder = value ?? defaultContextBuilder;
		}

		private static Func<IDiscordClient, IUserMessage, CommandContext> _contextBuilder = defaultContextBuilder!;

		private static readonly Func<IDiscordClient, IUserMessage, CommandContext> defaultContextBuilder
			= (client, message) => new CommandContext(client, message);

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

			SetCommandHandler();

			await Task.Delay(-1);
		}

#pragma warning disable CS1998
		public virtual async Task Ready()
		{
			CurrentState = ActiveState.Ready;

			ClientUserId = Client.CurrentUser.Id;

			RestApplication? info = await Client.GetApplicationInfoAsync();
			OwnerId = info.Owner.Id;
		}

		private void SetCommandHandler()
		{
			if (commandHandler == null)
				commandHandler = new CommandHandler(Client);
			else commandHandler.SetClient(Client);
		}

		public virtual async Task OnDisconnected(Exception arg) 
		{
		
		}

		public virtual async Task LogAsync(LogMessage message) 
		{
		
		}

		public virtual async Task OnJoinedGuild(SocketGuild guildJoined) 
		{
		
		}

		public virtual async Task OnLeftGuild(SocketGuild guildLeft) 
		{
		
		}

		public virtual async Task OnReactionAdded(Cacheable<IUserMessage, ulong> message,
			Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
		{

		}

		public virtual async Task OnReactionRemoved(Cacheable<IUserMessage, ulong> message,
			Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
		{

		}

#pragma warning restore CS1998
	}
}
