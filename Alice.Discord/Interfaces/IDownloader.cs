using System.Threading.Tasks;
using Discord.WebSocket;

namespace Alice.Discord.Interfaces {
	public interface IDownloader {
		bool IsValidUri(string uri);
		Task Download(SocketUserMessage message);
	}
}