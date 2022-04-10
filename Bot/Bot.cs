using Discord.Bot.Handler;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Discord.Bot
{
	public class Bot : IBotEvents
	{
		public static Bot Instance
		{
			get
			{
				if (_instance == null)
				{

				}

				return _instance;
			}
		}

		private static Bot _instance;

		public enum ActiveState
		{
			Booting,
			Paused,
			Ready,
			Updating,
			Exiting
		}

		public ulong ClientUserId { get; }

		public ulong OwnerId;

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
		}

		public virtual async Task Start(string token)
		{
			await Client.LoginAsync(TokenType.Bot, token);
			await Client.StartAsync();

			SetCommandHandler();
		}

		public virtual async Task Ready()
		{
			CurrentState = ActiveState.Ready;
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
	}
}
