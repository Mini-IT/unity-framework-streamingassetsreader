using System.IO;
using System.Text.RegularExpressions;

namespace MiniIT.Unity.StreamingAssets
{
	public static class PathUtil
	{
		public static string NormalizePath(string path, bool endingSlashNeeded)
		{
			path = FixSlashes(path);

			if (path.Length == 0)
			{
				return "/";
			}

			if (path[0] != '/')
			{
				path = "/" + path;
			}

			if (path.Length > 1)
			{
				if (path[path.Length - 1] == '/') // ends with slash
				{
					if (!endingSlashNeeded)
					{
						path = path.Substring(0, path.Length - 1);
					}
				}
				else if (endingSlashNeeded) // does not end with slash
				{
					path += "/";
				}
			}

			return path;
		}

		public static string FixSlashes(string path)
		{
			// Replace back slashes and slash sequences to single forward slashes
			return new Regex(@"(\\+|/+)").Replace(path, "/");
		}

		public static Regex WildcardToRegex(string pattern)
		{
			return new Regex("^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$", RegexOptions.IgnoreCase);
		}

		public static string GetFullPath(string streamingAssetsPath, string path)
		{
			if (path.StartsWith(streamingAssetsPath))
			{
				return path;
			}

			return Path.Combine(streamingAssetsPath, path);
		}

		public static string GetRelativePath(string streamingAssetsPath, string path)
		{
			if (path.StartsWith(streamingAssetsPath))
			{
				return path.Substring(streamingAssetsPath.Length);
			}
			return path;
		}
	}
}
