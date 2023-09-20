using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

namespace MiniIT.Unity
{
	public class AndroidStreamingAssetsReader : IStreamingAssetsReader
	{
		public async UniTask<string> ReadTextAsync(string path, CancellationToken cancellationToken = default)
		{
			var (isCancelled, downloadHandler) = await ReadAsync(path, cancellationToken).SuppressCancellationThrow();
			return downloadHandler?.text ?? null;
		}

		public async UniTask<byte[]> ReadBytesAsync(string path, CancellationToken cancellationToken = default)
		{
			var (isCancelled, downloadHandler) = await ReadAsync(path, cancellationToken).SuppressCancellationThrow();
			return downloadHandler?.data ?? null;
		}

		private async UniTask<DownloadHandler> ReadAsync(string path, CancellationToken cancellationToken = default)
		{
			using (var request = new UnityWebRequest(path))
			{
				request.downloadHandler = new DownloadHandlerBuffer();
				
				await request.SendWebRequest()
					.WithCancellation(cancellationToken); // automatically calls request.Abort() on cancellation

				if (request.result == UnityWebRequest.Result.Success)
				{
					return request.downloadHandler;
				}
			}

			return null;
		}
	}
}
