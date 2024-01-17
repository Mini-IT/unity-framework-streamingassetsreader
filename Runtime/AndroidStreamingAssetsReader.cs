using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
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
				return;
			}

			_streamingAssetsPath = path;
			await LoadCatalog();
		}

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

		public bool FileExists(string path)
		{
			if (_catalog == null)
			{
				return false;
			}

			path = NormalizePath(path);
			return _catalog.Contains(path);
		}

		public bool DirectoryExists(string path)
		{
			if (_catalog == null)
			{
				return false;
			}

			path = NormalizePath(path) + "/";
			foreach (var line in _catalog)
			{
				if (line.StartsWith(path))
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

			path = NormalizePath(path) + "/";

			Predicate<string> filter;
			if (string.IsNullOrEmpty(searchPattern) || searchPattern == "*")
			{
				filter = null;
			}
			else if (searchPattern.IndexOf('*') >= 0 || searchPattern.IndexOf('?') >= 0)
			{
				var regex = WildcardToRegex(searchPattern);
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
			string text = await ReadTextAsync(CATALOG_FILE_NAME);
			List<string> list = new List<string>();
			using (var reader = new StreamReader(text))
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

		private string NormalizePath(string path)
		{
#if UNITY_2021_1_OR_NEWER
			const char SLASH = '/';
#else
			const string SLASH = "/";
#endif

			path = path.Replace("\\", "/");

			int start = 0;
			int length = path.Length;
			if (path.StartsWith(SLASH))
			{
				start = 1;
				length--;
			}
			if (path.EndsWith(SLASH))
			{
				length--;
			}
			return path.Substring(start, length);
		}

		public static Regex WildcardToRegex(string pattern)
		{
			return new Regex("^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$", RegexOptions.IgnoreCase);
		}
	}
}
