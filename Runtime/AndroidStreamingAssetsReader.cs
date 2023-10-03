using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

namespace MiniIT.Unity
{
	public class AndroidStreamingAssetsReader : IStreamingAssetsReader
	{
		public async UniTask<string> ReadTextAsync(string path, CancellationToken cancellationToken = default)
		{
			using (var request = new UnityWebRequest(path))
			{
				request.downloadHandler = new DownloadHandlerBuffer();

				try
				{
					await request.SendWebRequest()
						.WithCancellation(cancellationToken) // automatically calls request.Abort() on cancellation
						.SuppressCancellationThrow();
				}
				catch (UnityWebRequestException e)
				{
					UnityEngine.Debug.LogError($"[{nameof(AndroidStreamingAssetsReader)}] Failed to read file '{path}': {e}");
				}

				if (request.result == UnityWebRequest.Result.Success)
				{
					return request.downloadHandler.text;
				}
			}

			return null;
		}

		public async UniTask<byte[]> ReadBytesAsync(string path, CancellationToken cancellationToken = default)
		{
			using (var request = new UnityWebRequest(path))
			{
				request.downloadHandler = new DownloadHandlerBuffer();

				try
				{
					await request.SendWebRequest()
						.WithCancellation(cancellationToken) // automatically calls request.Abort() on cancellation
						.SuppressCancellationThrow();
				}
				catch (UnityWebRequestException e)
				{
					UnityEngine.Debug.LogError($"[{nameof(AndroidStreamingAssetsReader)}] Failed to read file '{path}': {e}");
				}

				if (request.result == UnityWebRequest.Result.Success)
				{
					return request.downloadHandler.data;
				}
			}

			return null;
		}
	}
}
