using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace libgitface
{
	public class Downloader : IDownloader
	{
		static readonly string CacheLocation = Path.Combine (Environment.GetEnvironmentVariable ("HOME"), "Library", "Caches", "GityMcGitface", "buddy-test");

		public event EventHandler DownloadFailed;
		public event EventHandler DownloadStarted;
		public event EventHandler DownloadCompleted;
		public event EventHandler AllDownloadsCompleted;

		bool Downloading { get; set; }

		public Downloader()
		{
		}

		public async Task<string[]> Download (params string[] uris)
		{
			if (Downloading)
				throw new NotSupportedException ("This downloader is already downloading!");

			Downloading = true;
			var files = new List<string>();
			try {
				DownloadStarted?.Invoke (this, EventArgs.Empty);
				foreach (var uri in uris)
					files.Add (await DownloadFile (uri));
				DownloadCompleted?.Invoke (this, EventArgs.Empty);
			} catch {
				files.Clear ();
				DownloadFailed?.Invoke (this, EventArgs.Empty);
			}
			Downloading = false;

			return files.ToArray ();
		}

		async Task<string> DownloadFile (string uri)
		{
			var filename = Path.GetFileName (uri);
			var gitSha = Path.GetFileName (Path.GetDirectoryName (uri));
			var localPath = Path.Combine (CacheLocation, gitSha, filename);
			if (!File.Exists (localPath)) {
				Directory.CreateDirectory (Path.GetDirectoryName (localPath));
				Log.Raw ($"Downloading {uri} and saving it to {localPath}");
				using (var client = new HttpClient ())
				using (var response = await client.GetAsync (uri, HttpCompletionOption.ResponseHeadersRead))
				using (var localFile = File.OpenWrite (localPath))
					await response.Content.CopyToAsync (localFile);
			}

			Log.Raw ($"Returning cached download path: {localPath}");
			return localPath;
		}
	}
}
