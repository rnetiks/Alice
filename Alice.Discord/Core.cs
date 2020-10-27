using System;
using System.Collections.Generic;
using System.Linq;
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

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);
        }

        public static bool Contains(string i, IEnumerable<string> c) => c.All(i.Contains);

        private async Task OnMessageReceived(SocketMessage arg)
        {
            if (!(arg is SocketUserMessage message) || message.Author.IsBot)
            {
                return;
            }

            Console.WriteLine($"{message.Author.Username}: {message.Content}");
            int argPos = 0;
            if (message.HasStringPrefix("a!", ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                var context = new SocketCommandContext(_client, message);

                var result = await _commands.ExecuteAsync(context, argPos, _services);

                if (!result.IsSuccess)
                {
                    Console.WriteLine(result.ErrorReason); //Slight Performance Increase....
                }
            }
        }
    }
}
