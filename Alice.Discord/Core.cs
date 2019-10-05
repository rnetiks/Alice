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

        public async Task RunBotAsync(string token)
        {
            _client = new DiscordSocketClient();
            _commands = new CommandService();
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            _client.Log += Log;
            
            await RegisterCommandAsync();

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await _client.SetGameAsync("Devoloper Build");
        }

        private static async Task Log(LogMessage log)
        {
            Console.WriteLine(log.ToString());
        }

        private async Task RegisterCommandAsync()
        {
            _client.MessageReceived += OnMessageReceived;
            _client.ReactionAdded += OnReactionAdded;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);
        }
        
        private async Task OnHentaiSwitchPage(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reactions)
        {
            var f = Modules.BotUtility.interactiveMessages;
            for (int i = 0; i < f.Count; i++)
            {
                if (f[i].RequestAuthor == reactions.UserId && f[i].id == arg1.id;)
                {
                    f[i].currentPage = f[i].currentPage+=1;
                    if (f[i].currentPage <= f[i].ImageCount)
                    {
                        var c1 = _client.GetChannel(channel.Id);
                        var c = (c1 as IMessageChannel);
                        var m = c.GetMessageAsync(message.Id);
                        EmbedBuilder eb = new EmbedBuilder();
                        Console.WriteLine(($"{f[i].path}/{f[i].currentPage}.jpg"));
                        eb.WithImageUrl(($"{f[i].path}/{f[i].currentPage}.jpg"));
                        await ((IUserMessage) m.Result).ModifyAsync(msg => msg.Embed = eb.Build());
                        //(channel as IMessageChannel).SendMessageAsync(f[i].path+"/" + f[i].currentPage + ".jpg");
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

            if (message.Content == "p!catch alice")
            {
                await message.Channel.SendMessageAsync("Do not even try that... I will Bite you.");
                return;
            }

            if (message.Content.IndexOf("alice", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                await message.Channel.SendMessageAsync("no.");
                return;
            }

            Console.WriteLine(message.Content);
            int argPos = 0;
            if (message.HasStringPrefix("a!", ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                var context = new SocketCommandContext(_client, message);

                var result = await _commands.ExecuteAsync(context, argPos, _services);

                if (!result.IsSuccess)
                {
                    Console.WriteLine(result.ErrorReason);
                }
            }
        }
    }
}