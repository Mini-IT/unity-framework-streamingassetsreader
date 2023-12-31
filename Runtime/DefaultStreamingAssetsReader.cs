﻿using System;
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
#if UNITY_2021_1_OR_NEWER
					return await File.ReadAllTextAsync(path, cancellationToken);
#else
					return await UniTask.FromResult(File.ReadAllText(path))
						.AttachExternalCancellation(cancellationToken);
#endif
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
#if UNITY_2021_1_OR_NEWER
				return await File.ReadAllBytesAsync(path, cancellationToken);
#else
				return await UniTask.FromResult(File.ReadAllBytes(path))
					.AttachExternalCancellation(cancellationToken);
#endif
			}

			return null;
		}
	}
}
