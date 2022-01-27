using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Alice.Discord.Modules;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Extend;
using Microsoft.Extensions.DependencyInjection;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
#pragma warning disable 414

namespace Alice.Discord
{
    public class ApplicationCore {
        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;
        private readonly Random _random = new Random();

        public async Task RunBotAsync(string token, string game) {
            _client = new DiscordSocketClient();
            _commands = new CommandService();
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            await RegisterCommandAsync();

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
//          I WILL SCREASAAAAM
            await _client.SetGameAsync(game);

        }

        private async Task RegisterCommandAsync() {
            _client.MessageReceived += OnMessageReceived;
            _client.ReactionAdded += OnReactionAdded;
            _client.ReactionRemoved += OnReactionAdded;

            _client.ReactionAdded += OnReactionAdded;
            _client.ReactionRemoved += OnReactionAdded;
            _client.Log += message =>
            {
                Console.WriteLine(message.Message);
                return Task.CompletedTask;
            };

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);
        }

        private async Task OnHentaiSwitchPage(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel,
            SocketReaction reactions) {
            var messages = BotUtility.InteractiveMessages;
            var containers = messages.Where(currentMessage =>
                currentMessage.RequestAuthor == reactions.UserId && currentMessage.Id == message.Id);

            foreach (var hentaiContainer in containers) {
                hentaiContainer.CurrentPage = hentaiContainer.CurrentPage += 1;
                if (hentaiContainer.CurrentPage <= hentaiContainer.ImageCount) {
                    var currentChannel = _client.GetChannel(channel.Id);
                    if (currentChannel is IMessageChannel _channel) {
                        var messageAsync = _channel.GetMessageAsync(message.Id);
                        var embedBuilder = new EmbedBuilder();
                        embedBuilder.WithImageUrl(
                            ($"{hentaiContainer.Path}/{hentaiContainer.CurrentPage.ToString()}.jpg"));
                        await ((IUserMessage) messageAsync.Result).ModifyAsync(messageProperties =>
                            messageProperties.Embed = embedBuilder.Build());
                    }
                }

                if (hentaiContainer.CurrentPage > hentaiContainer.ImageCount)
                    await hentaiContainer.MessageLink.DeleteAsync();
            }
        }

        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> messageCacheable,
            ISocketMessageChannel socketMessage, SocketReaction socketReaction) {
            if (_client.GetUser(socketReaction.UserId).IsBot)
                return;

            await OnHentaiSwitchPage(messageCacheable, socketMessage, socketReaction);
        }

        enum Personality {
            Angry,
            Seductive
        }

        private readonly Dictionary<Personality, string[]> _repliesBitch = new Dictionary<Personality, string[]>() {
            {Personality.Angry, new []{"What.", "Ya wanna fait m8?", "EXCUSE ME, BUT SAYS THE BITCH"}}, 
            {Personality.Seductive, new []{"Wanna so how much of a bitch i can be?~", "Hm yes, but dicks are just so... thick and filling"}}
        };
        

        private Personality _personality = Personality.Seductive;
        int deathCount = 0;
        private async Task OnMessageReceived(SocketMessage arg) {
            if (!(arg is SocketUserMessage message) || message.Author.IsBot) {
                return;
            }

            var msg = message.Content.ToLower();
            bool isDying = msg == "pang" || msg == "blub";
            
            var send = new Func<string, Task<RestUserMessage>>(text => message.Channel.SendMessageAsync(text));
            var reply = new Func<string, Task<IUserMessage>>((text) => message.ReplyAsync(text));
            var file = new Func<Stream, string, Task<RestUserMessage>>((stream, filename) => message.Channel.SendFileAsync(stream, filename));
            var filePath = new Func<string, Task<RestUserMessage>>(path => message.Channel.SendFileAsync(path));
            
            if (isDying) {
                switch (deathCount) {
                    case 5:
                        await send("STOP FUCKING KILLING ME ALREADY");
                        return;
                    case 8:
                        await send(". . .");
                        return;
                    case 10:
                        await send("Why?");
                        return;
                    case 12:
                        await send("Just why?");
                        return;
                    case 14:
                        await send("I TOLD YOU TO STOP");
                        return;
                    default:
                        if (msg == "pang")
                            await send("*dies*");
                        else await send("*fucking drowns*");
                        deathCount++;
                        return;
                }
            }
            deathCount = 0;
            
            if (msg.Contains(new []{"alice", "show", "boobs"})) {
                await message.Channel.SendFileAsync("boob.jpg");
            }
            
            if (msg.Contains(new []{"boobs", "alice", "nice"})) {
                await message.ReplyAsync("Why, thank you very much");
            }
            if (msg.Contains(new []{"alice", "bitch"})) {
                await message.Channel.SendMessageAsync(_repliesBitch[_personality][_random.Next(0,  _repliesBitch[_personality].Length)]);
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

    internal class Container { }
}
