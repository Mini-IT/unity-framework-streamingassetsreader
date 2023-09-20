using System.Threading;
using Cysharp.Threading.Tasks;

namespace MiniIT.Unity
{
	public interface IStreamingAssetsReader
	{
		UniTask<string> ReadTextAsync(string path, CancellationToken cancellationToken = default);
		UniTask<byte[]> ReadBytesAsync(string path, CancellationToken cancellationToken = default);
	}
}
