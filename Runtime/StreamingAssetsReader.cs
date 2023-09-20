using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MiniIT.Unity
{
	public static class StreamingAssetsReader
	{
		private static IStreamingAssetsReader s_instance;

		private static IStreamingAssetsReader GetInstance()
		{
#if (UNITY_ANDROID || UNITY_WEBGL) && !UNITY_EDITOR
			s_instance ??= new AndroidStreamingAssetsReader();
#else
			s_instance ??= new DefaultStreamingAssetsReader();
#endif
			return s_instance;
		}

		private static string s_streamingAssetsPath;

		/// <summary>
		/// Must be called from Unity main thread
		/// </summary>
		public static void Initialize()
		{
			s_streamingAssetsPath = Application.streamingAssetsPath;
		}

		public static async UniTask<string> ReadTextAsync(string path, CancellationToken cancellationToken = default)
		{
			string fullPath = GetFullPath(path);
			return await GetInstance().ReadTextAsync(fullPath, cancellationToken);
		}

		public static async UniTask<byte[]> ReadBytesAsync(string path, CancellationToken cancellationToken = default)
		{
			string fullPath = GetFullPath(path);
			return await GetInstance().ReadBytesAsync(fullPath, cancellationToken);
		}

		private static string GetFullPath(string path)
		{
			if (string.IsNullOrEmpty(s_streamingAssetsPath))
			{
				// This will cause an error if called not from Unity main thread
				Initialize();
			}

			return Path.Combine(s_streamingAssetsPath, path);
		}
	}
}
