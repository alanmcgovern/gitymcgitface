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

		public string OldAndroidSha;
		public string OldAndroidUrl;
		public string OldMacSha;
		public string OldMacUrl;
		public string OldIosSha;
		public string OldIosUrl;

		public string NewAndroidSha;
		public string NewAndroidUrl;
		public string NewMacSha;
		public string NewMacUrl;
		public string NewIosSha;
		public string NewIosUrl;
	}

	public class BumpProvisionatorDependenciesController
	{
		const string AndroidPrefix = "xamarin.android";
		const string IosPrefix = "xamarin.ios";
		const string MacPrefix = "xamarin.mac";

		static readonly Uri DesignerUri = new Uri ("https://github.com/xamarin/designer");
		static readonly Uri MonoDroidUri = new Uri ("https://github.com/xamarin/monodroid");
		static readonly Uri MacIosUri = new Uri ("https://github.com/xamarin/xamarin-macios");

		public GitClient Designer {
			get;
		}

		public GitClient MacIos {
			get;
		}

		public GitClient MonoDroid {
			get;
		}

		public string ProvisionatorFile => "bot-provisioning/dependencies.csx";

		public BumpProvisionatorDependenciesController (GitClient client)
		{
			Designer = client.WithRepository (new Repository (DesignerUri));
			MacIos = client.WithRepository (new Repository (MacIosUri));
			MonoDroid = client.WithRepository (new Repository (MonoDroidUri));
		}

		/// <summary>
		/// Returns null if no dependency needs updating, otherwise returns the new content for the `bot-provisioning/dependencies.csx` file.
		/// </summary>
		/// <returns>The bump dependencies.</returns>
		public async Task<ProvisionatorInfo> TryBumpDependencies ()
		{
			var info = new ProvisionatorInfo ();

			info.OldDependenciesCsx = await Designer.GetFileContent (ProvisionatorFile);

			info.OldAndroidUrl = info.NewAndroidUrl = GetUrl (AndroidPrefix, info.OldDependenciesCsx);
			info.OldIosUrl = info.NewIosUrl = GetUrl (IosPrefix, info.OldDependenciesCsx);
			info.OldMacUrl = info.NewMacUrl = GetUrl (MacPrefix, info.OldDependenciesCsx);

			info.OldAndroidSha = info.NewAndroidSha = GetSha (info.OldAndroidUrl);
			info.OldIosSha = info.NewIosSha = GetSha (info.OldIosUrl);
			info.OldMacSha = info.NewMacSha = GetSha (info.OldMacUrl);

			var statuses = Enumerable.Empty<Octokit.CommitStatus> ();
			try {
				var macIosHead = await MacIos.GetHeadSha ();
				var macIosStatuses = await MacIos.GetLatestStatuses (macIosHead, "PKG-");
				statuses = statuses.Concat (macIosStatuses);
			} catch {
				Console.WriteLine ($"Could not get statuses from {MacIos.Repository}");
			}
			try {
				var androidHead = await MonoDroid.GetHeadSha ();
				var androidStatuses = await MonoDroid.GetLatestStatuses (androidHead, "PKG-");
				statuses = statuses.Concat (androidStatuses);
			} catch {
				Console.WriteLine ($"Could not get statuses from {MonoDroid.Repository}");
			}

			var newUrls = statuses
				.Where (t => t.State == Octokit.CommitState.Success)
				.Select (t => t.TargetUrl.ToString ()).ToArray ();

			info.NewAndroidUrl = newUrls.Where (t => t.Contains (AndroidPrefix)).SingleOrDefault () ?? info.NewAndroidUrl;
			info.NewIosUrl = newUrls.Where (t => t.Contains (IosPrefix)).SingleOrDefault () ?? info.NewIosUrl;
			info.NewMacUrl = newUrls.Where (t => t.Contains (MacPrefix)).SingleOrDefault () ?? info.NewMacUrl;

			info.NewAndroidSha = GetSha (info.NewAndroidUrl);
			info.NewIosSha = GetSha (info.NewIosUrl);
			info.NewMacSha = GetSha (info.NewMacUrl);

			info.NewDependenciesCsx = info.OldDependenciesCsx
				.Replace (info.OldAndroidUrl, info.NewAndroidUrl)
				.Replace (info.OldIosUrl, info.NewIosUrl)
				.Replace (info.OldMacUrl, info.NewMacUrl);

			if (info.OldDependenciesCsx != info.NewDependenciesCsx)
				return info;

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