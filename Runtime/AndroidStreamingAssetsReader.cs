using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using MiniIT.Unity.StreamingAssets;
using UnityEngine;
using UnityEngine.Networking;

namespace MiniIT.Unity
{
	public class AndroidStreamingAssetsReader : IStreamingAssetsReader
	{
		private const string CATALOG_FILE_NAME = "__catalog__.txt";

		public bool IsInitialized => _catalog != null;

		private static string[] s_emptyArray;
		private string _streamingAssetsPath;
		private List<string> _catalog = null;

		public async UniTask Initialize(string path)
		{
			if (_streamingAssetsPath == path)
			{
				await UniTask.WaitUntil(() => IsInitialized);
				return;
			}

			_streamingAssetsPath = path;
			await LoadCatalog();
		}

		public async UniTask<string> ReadTextAsync(string fullPath, CancellationToken cancellationToken = default)
		{
			using (var request = UnityWebRequest.Get(fullPath))
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
					Debug.LogError($"[{nameof(AndroidStreamingAssetsReader)}] Failed to read file '{fullPath}': {e}");
				}

				if (request.result == UnityWebRequest.Result.Success)
				{
					return request.downloadHandler.text;
				}
			}

			return null;
		}

		public async UniTask<byte[]> ReadBytesAsync(string fullPath, CancellationToken cancellationToken = default)
		{
			using (var request = UnityWebRequest.Get(fullPath))
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
					Debug.LogError($"[{nameof(AndroidStreamingAssetsReader)}] Failed to read file '{fullPath}': {e}");
				}

				if (request.result == UnityWebRequest.Result.Success)
				{
					return request.downloadHandler.data;
				}
			}

			return null;
		}

		public bool FileExists(string path)
		{
			if (_catalog == null)
			{
				return false;
			}

			path = PathUtil.NormalizePath(path, false);
			for (int i = 0; i < _catalog.Count; i++)
			{
				if (_catalog[i] == path)
				{
					return true;
				}
			}
			return false;
		}

		public bool DirectoryExists(string path)
		{
			if (_catalog == null)
			{
				return false;
			}

			path = PathUtil.NormalizePath(path, true);
			for (int i = 0; i < _catalog.Count; i++)
			{
				if (_catalog[i].StartsWith(path))
				{
					return true;
				}
			}
			return false;
		}

		public string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
		{
			if (_catalog == null)
			{
				return s_emptyArray ??= new string[0];
			}

			path = PathUtil.NormalizePath(path, true);

			Predicate<string> filter;
			if (string.IsNullOrEmpty(searchPattern) || searchPattern == "*")
			{
				filter = null;
			}
			else if (searchPattern.IndexOf('*') >= 0 || searchPattern.IndexOf('?') >= 0)
			{
				var regex = PathUtil.WildcardToRegex(searchPattern);
				filter = (x) => regex.IsMatch(x);
			}
			else
			{
				filter = (x) => string.Compare(x, searchPattern, true) == 0;
			}

			List<string> results = new List<string>();

			for (int i = 0; i < _catalog.Count; ++i)
			{
				string filePath = _catalog[i];

				if (!filePath.StartsWith(path))
				{
					continue;
				}

				string fileName;

				int dirSeparatorIndex = filePath.LastIndexOf('/', filePath.Length - 1, filePath.Length - path.Length);
				if (dirSeparatorIndex >= 0)
				{
					if (searchOption == SearchOption.TopDirectoryOnly)
					{
						continue;
					}

					fileName = filePath.Substring(dirSeparatorIndex + 1);
				}
				else
				{
					fileName = filePath.Substring(path.Length);
				}

				// now do a match
				if (filter == null || filter(fileName))
				{
					results.Add(filePath.Substring(1));
				}
			}

			return results.ToArray();
		}

		private async UniTask LoadCatalog()
		{
			string filePath = PathUtil.GetFullPath(_streamingAssetsPath, CATALOG_FILE_NAME);
			byte[] content = await ReadBytesAsync(filePath);
			List<string> list = new List<string>();

			if (content == null || content.Length == 0)
			{
				Debug.LogError($"[{nameof(AndroidStreamingAssetsReader)}] StreamingAssets catalog is empty: {filePath}");
				_catalog = list;
				return;
			}

			using (var reader = new StreamReader(new MemoryStream(content)))
			{
				while (!reader.EndOfStream)
				{
					string line = reader.ReadLine();
					if (!string.IsNullOrWhiteSpace(line))
					{
						list.Add(line.Trim());
					}
				}
			}
			_catalog = list;
		}

		public string GetFullPath(string path)
		{
			return PathUtil.GetFullPath(_streamingAssetsPath, path);
		}
	}
}
