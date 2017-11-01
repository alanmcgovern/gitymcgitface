using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace libgitface
{
	public class ProvisionatorInfo {
		public string OldDependenciesCsx;
		public string NewDependenciesCsx;

		public string OldMacSha;
		public string OldMacUrl;
		public string OldIosSha;
		public string OldIosUrl;

		public string NewMacSha;
		public string NewMacUrl;
		public string NewIosSha;
		public string NewIosUrl;
	}

	public class BumpProvisionatorDependenciesController
	{
		const string IosPrefix = "xamarin.ios";
		const string MacPrefix = "xamarin.mac";

		static readonly Uri DesignerUri = new Uri ("https://github.com/xamarin/designer");
		static readonly Uri MacIosUri = new Uri ("https://github.com/xamarin/xamarin-macios");

		public GitClient Designer {
			get;
		}

		public GitClient MacIos {
			get;
		}

		public string ProvisionatorFile => "bot-provisioning/dependencies.csx";

		public BumpProvisionatorDependenciesController (GitClient client)
		{
			Designer = client.WithRepository (new Repository (DesignerUri));
			MacIos = client.WithRepository (new Repository (MacIosUri));
		}

		/// <summary>
		/// Returns null if no dependency needs updating, otherwise returns the new content for the `bot-provisioning/dependencies.csx` file.
		/// </summary>
		/// <returns>The bump dependencies.</returns>
		public async Task<ProvisionatorInfo> TryBumpDependencies ()
		{
			var info = new ProvisionatorInfo ();

			info.OldDependenciesCsx = await Designer.GetFileContent (ProvisionatorFile);
			info.OldIosUrl = GetUrl (IosPrefix, info.OldDependenciesCsx);
			info.OldMacUrl = GetUrl (MacPrefix, info.OldDependenciesCsx);
			info.OldIosSha = GetSha (info.OldIosUrl);
			info.OldMacSha = GetSha (info.OldMacUrl);

			var actions = new List<IAction> ();

			var head = await MacIos.GetHeadSha ();
			var statuses = (await MacIos.GetLatestStatuses (head, "PKG-")).ToArray ();
			statuses = statuses.Where (t => t.State == Octokit.CommitState.Success).ToArray ();
			if (statuses.Length == 2) {
				var newUrls = statuses.Select (t => t.TargetUrl.ToString ()).ToArray ();
				info.NewIosUrl = newUrls.Where (t => t.Contains (IosPrefix)).Single ();
				info.NewMacUrl = newUrls.Where (t => t.Contains (MacPrefix)).Single ();
				info.NewIosSha = GetSha (info.NewIosUrl);
				info.NewMacSha = GetSha (info.NewMacUrl);
				info.NewDependenciesCsx = info.OldDependenciesCsx
					.Replace (info.OldMacUrl, info.NewMacUrl)
					.Replace (info.OldIosUrl, info.NewIosUrl);

				if (info.OldDependenciesCsx != info.NewDependenciesCsx)
					return info;
			}
			return null;
		}

		static string GetSha (string url)
		{
			return Path.GetFileName (Path.GetDirectoryName (url));
		}

		static string GetUrl (string prefix, string fileContent)
		{
			// Find a line matching this format and extract the URL:
			// Item ("https://bosstoragemirror.azureedge.net/wrench/macios-mac-master/eb/eb35aaaa38f5cb962430ccbbec811d6b3876fc62/xamarin.mac-4.1.0.124.pkg");
			var parts = fileContent.Split (new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			var line = parts
				.Select (t => t.Trim ())
				.Where (t => t.StartsWith ("Item ("))
				.Where (t => t.Contains ("http://") || t.Contains ("https://"))
				.Where (t => t.Contains (prefix))
				.Single ();

			parts = line.Split ('"');
			if (parts.Length != 3)
				throw new NotSupportedException ($"We expected to find a url embedded inside \"{line}\" but we found nothing!");

			// The url should be the middle part :)
			return parts [1];
		}
	}
}