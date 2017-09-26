using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Octokit;

namespace libgitface.Actions
{
	public class VSMBuddyTestAction : IAction
	{
		GitClient Client { get; }
		IDownloader Downloader { get; }
		public string[] Grouping { get; }
		Octokit.PullRequest Pull { get; }
		public string ShortDescription => $"Buddy test {Client.Repository.Name}/{Pull.Title}";
		public string Tooltip => $"Buddy test {Pull}";

		public VSMBuddyTestAction (GitClient client, PullRequest pull, IDownloader downloader, params string[] grouping)
		{
			Client = client;
			Pull = pull;
			Downloader = downloader;
			Grouping = grouping;
		}

		public async void Execute ()
		{
			var mpacks = await Client.GetLatestStatuses (Pull.Head.Sha, "MPACK-");
			var uris = mpacks.Select (t => t.TargetUrl.ToString ()).ToArray ();
			var cachedFiles = await Downloader.Download (uris);

			var designerBaseInstallPath =  Path.Combine ("/Applications", "Visual Studio.app", "Contents", "Resources", "lib", "monodevelop", "AddIns", "designer");

			Directory.Delete (designerBaseInstallPath, true);
			Directory.CreateDirectory (designerBaseInstallPath);
			foreach (var file in cachedFiles) {
				var installPath = Path.Combine (designerBaseInstallPath, Path.GetFileName (file));
				Process.Start ("unzip", $"-o \"{file}\" -d \"{installPath}\"").WaitForExit ();
			}

			var vstool =  Path.Combine ("/Applications", "Visual Studio.app", "Contents", "MacOS", "vstool");
			Process.Start (vstool, "setup reg-build").WaitForExit ();

			var vsm =  Path.Combine ("/Applications", "Visual Studio.app", "Contents", "MacOS", "VisualStudio");
			Process.Start (vsm);
		}
	}
}
