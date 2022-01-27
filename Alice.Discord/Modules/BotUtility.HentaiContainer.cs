using Discord.Rest;

namespace Alice.Discord.Modules {
	public partial class BotUtility {
		public class HentaiContainer
		{
			public int ImageCount;
			public string Name;
			public ulong Id; //Int64 ID of the Message
			public int CurrentPage = 0; //0: Cover, 1-2147483647: Pages
			public string Path; //Absolute Path to the Gallery
			public ulong RequestAuthor; //Int64 ID of the User which has Requested the Gallery
			public RestUserMessage MessageLink { get; set; }
		}
	}
}