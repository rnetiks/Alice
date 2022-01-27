using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using Alice.DataFetcher;
using Discord;
using Discord.WebSocket;
using JetBrains.Annotations;

namespace Alice.Discord
{
    public class MangaReader
    {
        private const string NextPageEmote = "\u25B6";
        private const string PreviousPageEmote = "\u25C0";
        private const string KillEmote = "\u274C";
        
        private readonly DiscordSocketClient _bot;
        private readonly MangaInfo _manga;
        private readonly List<ulong> _allowedUsers;

        private ulong _listeningMessage;
        private int _currentPage;

        public MangaReader(DiscordSocketClient bot, MangaInfo manga, List<ulong> users)
        {
            _bot = bot;
            _manga = manga;
            _allowedUsers = users;
        }
        
        public MangaReader(DiscordSocketClient bot, MangaInfo manga, params ulong[] users)
        {
            if (users.Length == 0)
                throw new ArgumentException("Please specify at least one user", nameof(users));
            
            _bot = bot;
            _manga = manga;
            _allowedUsers = new List<ulong>(users);
        }

        private static EmbedBuilder FromMangaInfo(MangaInfo manga)
        {
            var embed = new EmbedBuilder {Title = manga.Title};


            if (manga.Artists.Length >= 1)
                embed.Author = new EmbedAuthorBuilder().WithName(manga.Artists[0]);

            embed.Color = Color.Teal;

            if (manga.Tags.Length > 0)
            {
                embed.Fields.Add(new EmbedFieldBuilder
                {
                    Name = "Tags",
                    Value = string.Join(" ", manga.Tags)
                });
            }

            if (manga.Categories.Length > 0)
            {
                embed.Fields.Add(new EmbedFieldBuilder
                {
                    Name = "Categories",
                    Value = string.Join(" ", manga.Categories)
                });
            }

            if (manga.Languages.Length > 0)
            {
                embed.Fields.Add(new EmbedFieldBuilder
                {
                    Name = "Languages",
                    Value = string.Join(" ", manga.Languages)
                });
            }

            return embed;
        }

        [UsedImplicitly]
        public async Task Create(ISocketMessageChannel channel)
        {
            var embed = FromMangaInfo(_manga);
            if (_manga.Cover != null)
            {
                embed.ImageUrl = _manga.Cover;
                embed.Footer = new EmbedFooterBuilder().WithText("Cover");
                _currentPage = -1;
            }
            else
            {
                embed.ImageUrl = _manga.Pages[0];
                embed.Footer = new EmbedFooterBuilder().WithText($"1 / {_manga.PageCount}");
                _currentPage = 0;
            }

            var msg = await channel.SendMessageAsync(null, false, embed.Build());
            
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
                var embed = FromMangaInfo(_manga);
                embed.ImageUrl = _manga.Pages[page];
                embed.Footer = new EmbedFooterBuilder().WithText($"{page + 1} / {_manga.PageCount}");

                msg.Embed = embed.Build();
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
                    if (_currentPage + 1 < _manga.PageCount)
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