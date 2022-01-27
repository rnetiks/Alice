using Newtonsoft.Json;

namespace Alice.Discord.Modules {
	public partial class BotUtility {
		public class DbImageGetClass
		{
			[JsonProperty("file_url")]
			public string Url;
            
			[JsonProperty("tag_string")]
			public string Tags;

			[JsonProperty("tag_string_artist")]
			public string Artist;
		}
	}
}