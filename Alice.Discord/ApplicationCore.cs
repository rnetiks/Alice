using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Alice.Discord.Modules;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
#pragma warning disable 414

namespace Alice.Discord
{
    public class Core {
        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;
        private readonly Random _random = new Random();
        
        public async Task RunBotAsync(string token, string game)
        {
            _client = new DiscordSocketClient();
            _commands = new CommandService();
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();
            
            await RegisterCommandAsync();

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
//          I WILL SCREAAAAAM
            await _client.SetGameAsync(game);

        }

        private async Task RegisterCommandAsync()
        {
            _client.MessageReceived += OnMessageReceived;
            _client.ReactionAdded += OnReactionAdded;
            _client.ReactionRemoved += OnReactionAdded;

            //_client.ReactionAdded += OnReactionAdded;
            //_client.ReactionRemoved += OnReactionAdded;
            _client.Log += message =>
            {
                Console.WriteLine(message.Message);
                return Task.CompletedTask;
            };

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);
        }
        
        private async Task OnHentaiSwitchPage(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reactions) {
            var messages = BotUtility.InteractiveMessages;
            var containers = messages.Where(currentMessage => currentMessage.RequestAuthor == reactions.UserId && currentMessage.Id == message.Id);

            foreach (var hentaiContainer in containers) {
                hentaiContainer.CurrentPage = hentaiContainer.CurrentPage+=1;
                if (hentaiContainer.CurrentPage <= hentaiContainer.ImageCount)
                {
                    var currentChannel = _client.GetChannel(channel.Id);
                    if (currentChannel is IMessageChannel _channel) {
                        var messageAsync = _channel.GetMessageAsync(message.Id);
                        var embedBuilder = new EmbedBuilder();
                        embedBuilder.WithImageUrl(($"{hentaiContainer.Path}/{hentaiContainer.CurrentPage.ToString()}.jpg"));
                        await ((IUserMessage) messageAsync.Result).ModifyAsync(messageProperties => messageProperties.Embed = embedBuilder.Build());
                    }
                }

                if (hentaiContainer.CurrentPage > hentaiContainer.ImageCount)
                    await hentaiContainer.MessageLink.DeleteAsync();
            }
        }
        
        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> messageCacheable, ISocketMessageChannel socketMessage, SocketReaction socketReaction)
        {
            if (_client.GetUser(socketReaction.UserId).IsBot)
                return;
            
            await OnHentaiSwitchPage(messageCacheable, socketMessage, socketReaction);
        }

        
        private readonly string[] _bitchReply = {"What.", "Ya wanna fait m8?", "EXCUSE ME, BUT SAYS THE BITCH", "I know you are one"};
        private async Task OnMessageReceived(SocketMessage arg) {
            if (!(arg is SocketUserMessage message) || message.Author.IsBot)
            {
                return;
            }

            if (message.Content.ToLowerInvariant() == "alice show me your boobs") {
                await message.Channel.SendFileAsync("boob.jpg");
            }

            if (message.Content == "still those are some nice boobs alice") {
                await message.ReplyAsync("Why, thank you very much");
            }
            if (message.Content.ToLowerInvariant() == "alice bitch") {
                await message.Channel.SendMessageAsync(_bitchReply[_random.Next(0, _bitchReply.Length)]);
            }
            
            int argPos = 0;
            if (message.HasStringPrefix("a!", ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                var context = new SocketCommandContext(_client, message);

                var result = await _commands.ExecuteAsync(context, argPos, _services);

                if (!result.IsSuccess) {
                    await message.Channel.SendMessageAsync($"{result.ErrorReason}");
                }
            }
        }
    }
    
}
