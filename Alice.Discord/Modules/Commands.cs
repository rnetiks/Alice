using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;

namespace Alice.Discord.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        #region Init
        
        HttpClient client = new HttpClient();
        
        #endregion
        
        [Command("post")]
        public async Task getPost(int id) {
            await Context.Channel.SendMessageAsync("What... no...");

        }
    }

}
