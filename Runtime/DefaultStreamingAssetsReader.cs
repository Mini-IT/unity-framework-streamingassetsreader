using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace MiniIT.Unity
{
	public class DefaultStreamingAssetsReader : IStreamingAssetsReader
	{
		public async UniTask<string> ReadTextAsync(string path, CancellationToken cancellationToken = default)
		{
			if (File.Exists(path))
			{
				try
				{
					return await File.ReadAllTextAsync(path, cancellationToken);
				}
				catch (OperationCanceledException)
				{
					// Suppress cancelation throw
				}
			}

			return null;
		}

		public async UniTask<byte[]> ReadBytesAsync(string path, CancellationToken cancellationToken = default)
		{
			if (File.Exists(path))
			{
				return await File.ReadAllBytesAsync(path, cancellationToken);
			}

			return null;
		}
	}
}
