using System;
using System.Threading.Tasks;

namespace libgitface
{
	public interface IDownloader
	{
		event EventHandler DownloadFailed;
		event EventHandler DownloadStarted;
		event EventHandler DownloadCompleted;
		Task<string[]> Download (params string[] uris);
	}
}