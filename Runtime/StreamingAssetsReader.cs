using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MiniIT.Unity
{
	public static class StreamingAssetsReader
	{
		public static bool IsInitialized => s_instance?.IsInitialized ?? false;

		private static IStreamingAssetsReader s_instance;

		private static IStreamingAssetsReader GetInstance()
		{
#if (UNITY_ANDROID || UNITY_WEBGL) && !UNITY_EDITOR
			return s_instance ??= new AndroidStreamingAssetsReader();
#else
			return s_instance ??= new DefaultStreamingAssetsReader();
#endif
		}

		/// <summary>
		/// Must be called from Unity main thread
		/// </summary>
		public static void Initialize(Action callback = null)
		{
			InternalInitializeAsync(callback).Forget();
		}

		public static async UniTask InitializeAsync()
		{
			await InternalInitializeAsync();
		}

		private static async UniTask InternalInitializeAsync(Action callback = null)
		{
			await GetInstance().Initialize(Application.streamingAssetsPath);
			callback?.Invoke();
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

		public static async UniTask<bool> CopyToFileAsync(string inputPath, string outputPath, CancellationToken cancellationToken = default)
		{
			string fullPath = GetFullPath(inputPath);
			return await GetInstance().CopyToFileAsync(fullPath, outputPath, cancellationToken);
		}

		public static bool FileExists(string path)
		{
			return GetInstance().FileExists(path);
		}
		public static bool DirectoryExists(string path)
		{
			return GetInstance().DirectoryExists(path);
		}

		public static string[] GetFiles(string path)
		{
			return GetFiles(path, null);
		}

		public static string[] GetFiles(string path, string searchPattern)
		{
			return GetFiles(path, searchPattern, SearchOption.TopDirectoryOnly);
		}

		public static string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			return GetInstance().GetFiles(path, searchPattern, searchOption);
		}

		private static string GetFullPath(string path)
		{
			if (!IsInitialized)
			{
				// This will cause an error if called not from Unity main thread
				Initialize();
			}

			return GetInstance().GetFullPath(path);
		}
	}
}
