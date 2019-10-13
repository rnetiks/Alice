using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Alice.Discord
{
    public class MangaReader
    {
        private const string NextPageEmote = "\u25B6";
        private const string PreviousPageEmote = "\u25C0";
        private const string KillEmote = "\u274C";
        
        private readonly DiscordSocketClient _bot;
        private readonly List<string> _pages;
        private readonly List<ulong> _allowedUsers;

        private ulong _listeningMessage;
        private int _currentPage;

        public MangaReader(DiscordSocketClient bot, List<string> pages, List<ulong> users)
        {
            _bot = bot;
            _pages = pages;
            _allowedUsers = users;
        }
        
        public MangaReader(DiscordSocketClient bot, List<string> pages, params ulong[] users)
        {
            if (users.Length == 0)
                throw new ArgumentException("Please specify atleast one user", nameof(users));
            
            _bot = bot;
            _pages = pages;
            _allowedUsers = new List<ulong>(users);
        }

        public async Task Create(ISocketMessageChannel channel, string mangaWebsite)
        {
            var embed = new EmbedBuilder
            {
                ImageUrl = _pages[0]
            }.Build();
            
            var msg = await channel.SendMessageAsync(null, false, embed);
            
            await msg.AddReactionAsync(new Emoji(PreviousPageEmote));
            await msg.AddReactionAsync(new Emoji(NextPageEmote));
            await msg.AddReactionAsync(new Emoji(KillEmote));
            
            Bind(msg);
        }

        private void Bind(IMessage message)
        {
            _listeningMessage = message.Id;

            _bot.ReactionAdded += OnReactionAdded;
            _bot.MessageDeleted += OnMessageDeleted;
        }

        private void Unbind()
        {
            _listeningMessage = 0;

            _bot.ReactionAdded -= OnReactionAdded;
            _bot.MessageDeleted -= OnMessageDeleted;
        }

        private async Task SetPage(int page, IUserMessage message)
        {
            await message.ModifyAsync(msg =>
            {
                msg.Embed = new EmbedBuilder
                {
                    ImageUrl = _pages[page]
                }.Build();
            });
            
            _currentPage = page;
        }

        private void FinishedReading(IUserMessage message)
        {
            throw new NotImplementedException();
        }

        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (message.Id != _listeningMessage)
                return;
                    
            IUser sender = _bot.GetUser(reaction.UserId);
            var msg = (IUserMessage) await channel.GetMessageAsync(message.Id);
            
            await msg.RemoveReactionAsync(reaction.Emote, sender);
            
            if (sender.IsBot)
                return;

            if (!_allowedUsers.Contains(reaction.UserId))
                return;

            switch (reaction.Emote.Name)
            {
                case NextPageEmote:
                    if (_currentPage + 1 < _pages.Count)
                    {
                        await SetPage(_currentPage + 1, msg);
                    }
                    else
                    {
                        ++_currentPage;
                        FinishedReading(msg);
                    }
                    break;
                case PreviousPageEmote:
                    if (_currentPage - 1 >= 0)
                    {
                        await SetPage(_currentPage - 1, msg);
                    }
                    break;
                case KillEmote:
                    await msg.DeleteAsync(); // This calls OnMessageDeleted
                    break;
            }
        }

        private Task OnMessageDeleted(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
        {
            if (message.Id == _listeningMessage)
                Unbind();

            return Task.CompletedTask;
        }
    }
}