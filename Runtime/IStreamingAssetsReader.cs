using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace MiniIT.Unity
{
	public interface IStreamingAssetsReader
	{
		bool IsInitialized { get; }
		UniTask Initialize(string path);
		UniTask<string> ReadTextAsync(string path, CancellationToken cancellationToken = default);
		UniTask<byte[]> ReadBytesAsync(string path, CancellationToken cancellationToken = default);

		/// <summary>
		/// This is a special method for copying large files. It writes read bytes directly to file,
		/// so the memory usage is low regardless of the size of the file being downloaded.
		/// The distinction from other methods is that you cannot get data out of this one, all data is saved to a file.
		/// </summary>
		/// <param name="inputPath">Path to the file in Streaming Assets to read</param>
		/// <param name="outputPath">Path to the ecternal file to be written to</param>
		/// <param name="cancellationToken"></param>
		/// <returns><c>true</c> idf succeeded</returns>
		UniTask<bool> CopyToFileAsync(string inputPath, string outputPath, CancellationToken cancellationToken = default);
		bool FileExists(string path);
		bool DirectoryExists(string path);
		string[] GetFiles(string path, string searchPattern, SearchOption searchOption);
		string GetFullPath(string path);
	}
}
