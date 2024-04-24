using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using MiniIT.Unity.StreamingAssets;

namespace MiniIT.Unity
{
	public class DefaultStreamingAssetsReader : IStreamingAssetsReader
	{
		public bool IsInitialized => _streamingAssetsPath != null;

		private string _streamingAssetsPath;
		private static string[] s_emptyArray;

		public UniTask Initialize(string path)
		{
			_streamingAssetsPath = path;
			return UniTask.CompletedTask;
		}

		public async UniTask<string> ReadTextAsync(string path, CancellationToken cancellationToken = default)
		{
			if (!File.Exists(path))
			{
				return null;
			}

#if UNITY_2021_1_OR_NEWER
			return await File.ReadAllTextAsync(path, cancellationToken);
#else
			return await UniTask.FromResult(File.ReadAllText(path))
				.AttachExternalCancellation(cancellationToken);
#endif
		}

		public async UniTask<byte[]> ReadBytesAsync(string path, CancellationToken cancellationToken = default)
		{
			if (!File.Exists(path))
			{
				return null;
			}

#if UNITY_2021_1_OR_NEWER
			return await File.ReadAllBytesAsync(path, cancellationToken);
#else
			return await UniTask.FromResult(File.ReadAllBytes(path))
				.AttachExternalCancellation(cancellationToken);
#endif
		}

		public async UniTask<bool> CopyToFileAsync(string inputPath, string outputPath, CancellationToken cancellationToken = default)
		{
			if (!File.Exists(inputPath))
			{
				return false;
			}

			using (var input = File.OpenRead(inputPath))
			{
				using (var output = File.OpenWrite(outputPath))
				{
#if UNITY_2021_1_OR_NEWER
					await input.CopyToAsync(output, cancellationToken);
#else
					await input.CopyToAsync(output, 4096, cancellationToken);
#endif
				}
			}

			return true;
		}

		public bool FileExists(string path)
		{
			path = PathUtil.GetFullPath(_streamingAssetsPath, path);
			return File.Exists(path);
		}

		public bool DirectoryExists(string path)
		{
			path = PathUtil.GetFullPath(_streamingAssetsPath, path);
			return Directory.Exists(path);
		}

		public string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
		{
			if (!Directory.Exists(_streamingAssetsPath))
			{
				return s_emptyArray ??= new string[0];
			}

			if (PathUtil.FixSlashes(path) == "/")
			{
				path = "";
			}

			string[] files = Directory.GetFiles(PathUtil.GetFullPath(_streamingAssetsPath, path), searchPattern ?? "*", searchOption);

			for (int i = 0; i < files.Length; ++i)
			{
				files[i] = PathUtil.FixSlashes(files[i].Substring(_streamingAssetsPath.Length + 1));
			}

#if UNITY_EDITOR
			// purge meta files
			int count = 0;
			for (int i = 0; i < files.Length; ++i)
			{
				if (!files[i].EndsWith(".meta"))
				{
					files[count++] = files[i];
				}
			}
			Array.Resize(ref files, count);
#endif
			return files;
		}

		public string GetFullPath(string path)
		{
			return PathUtil.GetFullPath(_streamingAssetsPath, path);
		}
	}
}
