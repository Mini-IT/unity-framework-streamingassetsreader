using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace MiniIT.Unity
{
	public interface IStreamingAssetsReader
	{
		void Initialize(string path);
		UniTask<string> ReadTextAsync(string path, CancellationToken cancellationToken = default);
		UniTask<byte[]> ReadBytesAsync(string path, CancellationToken cancellationToken = default);
		bool FileExists(string path);
		bool DirectoryExists(string path);
		string[] GetFiles(string path, string searchPattern, SearchOption searchOption);
	}
}
