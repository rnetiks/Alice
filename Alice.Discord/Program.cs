using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Alice.Discord
{
    class Program
    {
        static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;
		public const string Login = "";
        public async Task RunBotAsync()
        {
            for (int i = 0; i < 666; i++)
            {
                Console.WriteLine($"Starting Bot, please wait for the Devil ({i+1}/666)");
            }
            Thread.Sleep(5000);
            _client = new DiscordSocketClient();
            _commands = new CommandService();
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            _client.Log += _client_Log;
            await RegisterCommandAsync();

            await _client.LoginAsync(TokenType.Bot, Login);

            await _client.StartAsync();

            await _client.SetGameAsync("Devoloper Build");
            await Task.Delay(-1);
        }

        private Task _client_Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        public async Task RegisterCommandAsync()
        {
            _client.ReactionAdded += _client_ReactionAdded;

            //if a user joins the server
            _client.UserJoined += _client_UserJoined;

            //if bot receives a message
            _client.MessageReceived += _client_MessageReceived;
            _client.ReactionAdded += _client_ReactionAdded1;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);
        }
        private void _client_SwitchHentaiPage(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            var f = Modules.BotUtility.l;
            for (int i = 0; i < f.Count; i++)
            {
                if (f[i].RequestAuthor == arg3.UserId)
                {
                    f[i].currentPage = f[i].currentPage+=1;
                    if (f[i].currentPage <= f[i].ImageCount)
                    {
                        var channel = _client.GetChannel(arg2.Id);
                        var c = (channel as IMessageChannel);
                        var m = c.GetMessageAsync(arg1.Id);
                        EmbedBuilder eb = new EmbedBuilder();
                        Console.WriteLine(($"{f[i].path}/{f[i].currentPage}.jpg"));
                        eb.WithImageUrl(($"{f[i].path}/{f[i].currentPage}.jpg"));
                        try
                        {

                            (m.Result as IUserMessage).ModifyAsync(msg => msg.Embed = eb.Build());
                        }
                        catch (Exception)
                        {

                            throw;
                        }
                        //(channel as IMessageChannel).SendMessageAsync(f[i].path+"/" + f[i].currentPage + ".jpg");
                    }
                }
            }
        }
        private Task _client_ReactionAdded1(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if (_client.GetUser(arg3.UserId).IsBot)
            {
                return Task.CompletedTask;
            }
            _client_SwitchHentaiPage(arg1, arg2, arg3);
            return Task.CompletedTask;
        }

        private Task _client_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            return Task.CompletedTask;
        }

        private Task _client_UserJoined(SocketGuildUser arg)
        {
            return Task.CompletedTask;
        }
        public static string[] cur = new string[]
        {
            "Fuck you",
            "Shut up",
            "Die already",
            "I will kill you while u sleep",
            "Justin Bieber sucks, but you suck just as much Dick",
            "... Die",
            "Kill Yourself",
            "My hate for you is Eternal",
            "OH SHUT UP ALREADY",
            "You know... there is this easy trick... take a Toaster... inside a Bath... TO FUCKING KILL YOURSELF",
            "Death is the only Answer for you",
            "May i suggest, that you should suck less Dicks?",
            "Fucking Gaylord",
            "I am no longer Proud...\n/kill"
        };
        public static string[] lik = new string[]
        {

        };
        Random rdm = new Random();
        private async Task _client_MessageReceived(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            if (message == null || message.Author.IsBot)
            {
                return;
            }
            if (false)
            {
                await message.Channel.SendMessageAsync(cur[rdm.Next(cur.Length)]);
                return;
            }
            if (message.Content == "p!catch alice")
            {
                await message.Channel.SendMessageAsync("Do not even try that... I will Bite you.");
                return;
            }
            if (message.Content.ToLower().Contains("alice"))
            {
                await message.Channel.SendMessageAsync("no.");
                return;
            }
            Console.WriteLine(message.Content);
            int argPos = 0;
            if (message.HasStringPrefix($"a!", ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))
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
