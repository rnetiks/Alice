using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
//TODO REWRITE THIS WHOLE FUCKING SHIT.
namespace Alice.Discord {
	public struct ProgressArgs {
		public readonly float? Progress;
		public readonly long? TotalBytes;
		public readonly long? LoadedBytes;

		public ProgressArgs(long? loadedBytes, long? totalBytes, float? progress) {
			LoadedBytes = loadedBytes;
			TotalBytes = totalBytes;
			Progress = progress;
		}

	}

	public class HttpClientExtension {
		public delegate void OnFinish(object sender, EventArgs args);

		public delegate void OnLoad(object sender, HttpContentHeaders args);

		public delegate void OnProgress(object sender, ProgressArgs args);

		public delegate void OnData(object sender, byte[] args);

		public event OnLoad Load;
		
		public event OnFinish Finish;
		
		public event OnProgress Progress;
		
		public event OnData Data;
		
		private const int DefaultBuffer = 1024;
		
		public async Task AsyncSend(HttpClient httpClient, HttpRequestMessage requestMessage, int maxBuffer = DefaultBuffer) {
			int read;
			var offset = 0;
			var request = await httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);
			Load?.Invoke(this, request.Content.Headers);
			var length = request.Content.Headers.ContentLength;

			await using var responseStream = await request.Content.ReadAsStreamAsync();
			var responseBuffer = new byte[maxBuffer];
			do {
				read = await responseStream.ReadAsync(responseBuffer, 0, responseBuffer.Length);
				Progress?.Invoke(this, new ProgressArgs(offset, length, (float) offset / length * 100));
				offset += read;
				Data?.Invoke(this, responseBuffer);
			} while (read != 0);
			Finish?.Invoke(this, EventArgs.Empty);
		}
	}
}