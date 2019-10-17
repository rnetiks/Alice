using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Alice.Discord
{
    public class Core
    {
        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;
        
        public async Task RunBotAsync(string token, string game)
        {
            _client = new DiscordSocketClient();
            _commands = new CommandService();
            _services = new ServiceCollection()
                .AddSingleton<DiscordSocketClient>(_client)
                .AddSingleton<CommandService>(_commands)
                .BuildServiceProvider();
            
            await RegisterCommandAsync();

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await _client.SetGameAsync(game);
        }

        private async Task RegisterCommandAsync()
        {
            _client.MessageReceived += OnMessageReceived;
            _client.ReactionAdded += OnReactionAdded;
            _client.ReactionRemoved += OnReactionAdded;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);
        }
        
        private async Task OnHentaiSwitchPage(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reactions)
        {
            var f = Modules.BotUtility.InteractiveMessages;
            for (int i = 0; i < f.Count; i++)
            {
                if (f[i].RequestAuthor == reactions.UserId && f[i].Id == message.Id)
                {
                    f[i].CurrentPage = f[i].CurrentPage+=1;
                    if (f[i].CurrentPage <= f[i].ImageCount)
                    {
                        var c1 = _client.GetChannel(channel.Id);
                        var c = (c1 as IMessageChannel);
                        var m = c.GetMessageAsync(message.Id);
                        EmbedBuilder eb = new EmbedBuilder();
                        eb.WithImageUrl(($"{f[i].Path}/{f[i].CurrentPage}.jpg"));
                        await ((IUserMessage) m.Result).ModifyAsync(msg => msg.Embed = eb.Build());
                    }
                }
            }
        }
        
        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if (_client.GetUser(arg3.UserId).IsBot)
                return;
            
            await OnHentaiSwitchPage(arg1, arg2, arg3);
        }

        private async Task OnMessageReceived(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            if (message == null || message.Author.IsBot)
            {
                return;
            }

            int argPos = 0;
            if (message.HasStringPrefix("a!", ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                var context = new SocketCommandContext(_client, message);

                var result = await _commands.ExecuteAsync(context, argPos, _services);

                if (!result.IsSuccess)
                {
                    //Console.WriteLine(result.ErrorReason); //Slight Performance Increase....
                }
            }
        }
    }
}
