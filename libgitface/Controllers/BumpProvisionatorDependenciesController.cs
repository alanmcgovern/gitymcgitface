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
		public string OldVSMSha;
		public string OldVSMUrl;

		public string NewAndroidSha;
		public string NewAndroidUrl;
		public string NewMacSha;
		public string NewMacUrl;
		public string NewIosSha;
		public string NewIosUrl;
		public string NewVSMSha;
		public string NewVSMUrl;
	}

	public class BumpProvisionatorDependenciesController
	{
		const string AndroidPrefix = "xamarin.android";
		const string IosPrefix = "xamarin.ios";
		const string MacPrefix = "xamarin.mac";
		const string VisualStudioMacPrefix = "VisualStudioForMac";

		static readonly Uri MonoDroidUri = new Uri ("https://github.com/xamarin/monodroid");
		static readonly Uri MacIosUri = new Uri ("https://github.com/xamarin/xamarin-macios");
		static readonly Uri VisualStudioMacUri = new Uri ("https://github.com/mono/monodevelop");

		public GitClient Designer {
			get;
		}

		public GitClient MacIos {
			get;
		}

		public GitClient MonoDroid {
			get;
		}

		public GitClient VisualStudioMac {
			get;
		}

		public string ProvisionatorFile => "bot-provisioning/dependencies.csx";

		public BumpProvisionatorDependenciesController (GitClient client)
		{
			Designer = client;
			MacIos = client.WithRepository (new Repository (MacIosUri));
			MonoDroid = client.WithRepository (new Repository (MonoDroidUri));
			VisualStudioMac = client.WithRepository (new Repository (VisualStudioMacUri));
		}

		/// <summary>
		/// Returns null if no dependency needs updating, otherwise returns the new content for the `bot-provisioning/dependencies.csx` file.
		/// </summary>
		/// <returns>The bump dependencies.</returns>
		public async Task<ProvisionatorInfo> TryBumpDependencies ()
		{
			var info = new ProvisionatorInfo ();

			info.NewDependenciesCsx = info.OldDependenciesCsx = await Designer.GetFileContent (ProvisionatorFile);

			info.OldAndroidUrl = info.NewAndroidUrl = GetUrl (AndroidPrefix, info.OldDependenciesCsx);
			info.OldIosUrl = info.NewIosUrl = GetUrl (IosPrefix, info.OldDependenciesCsx);
			info.OldMacUrl = info.NewMacUrl = GetUrl (MacPrefix, info.OldDependenciesCsx);
			info.OldVSMUrl = info.NewVSMUrl = GetUrl (VisualStudioMacPrefix, info.OldDependenciesCsx);

			info.OldAndroidSha = info.NewAndroidSha = GetSha (info.OldAndroidUrl);
			info.OldIosSha = info.NewIosSha = GetSha (info.OldIosUrl);
			info.OldMacSha = info.NewMacSha = GetSha (info.OldMacUrl);
			info.OldVSMSha = info.NewVSMSha = GetSha (info.OldVSMUrl);

			var statuses = Enumerable.Empty<Octokit.CommitStatus> ()
			                         .Concat (await GetLatestStatuses (MacIos, "PKG-"))
			                         .Concat (await GetLatestStatuses (MonoDroid, "PKG-"))
			                         .Concat (await GetLatestStatuses (VisualStudioMac, "DMG-"))
			                         .Where (t => t.State == Octokit.CommitState.Success);

			var newUrls = statuses
				.Select (t => t.TargetUrl.ToString ()).ToArray ();

			if (info.OldAndroidUrl != null)
				info.NewAndroidUrl = newUrls.Where (t => t.Contains (AndroidPrefix)).SingleOrDefault () ?? info.NewAndroidUrl;
			if (info.OldIosUrl != null)
				info.NewIosUrl = newUrls.Where (t => t.Contains (IosPrefix)).SingleOrDefault () ?? info.NewIosUrl;
			if (info.OldMacUrl != null)
				info.NewMacUrl = newUrls.Where (t => t.Contains (MacPrefix)).SingleOrDefault () ?? info.NewMacUrl;
			if (info.OldVSMUrl != null)
				info.NewVSMUrl = newUrls.Where (t => t.Contains (VisualStudioMacPrefix)).SingleOrDefault () ?? info.NewVSMUrl;

			info.NewAndroidSha = GetSha (info.NewAndroidUrl);
			info.NewIosSha = GetSha (info.NewIosUrl);
			info.NewMacSha = GetSha (info.NewMacUrl);
			info.NewVSMSha = GetSha (info.NewVSMUrl);

			if (info.OldAndroidUrl != info.NewAndroidUrl)
				info.NewDependenciesCsx = info.NewDependenciesCsx.Replace (info.OldAndroidUrl, info.NewAndroidUrl);
			if (info.OldIosUrl != info.NewIosUrl)
				info.NewDependenciesCsx = info.NewDependenciesCsx.Replace (info.OldIosUrl, info.NewIosUrl);
			if (info.OldMacUrl != info.NewMacUrl)
				info.NewDependenciesCsx = info.NewDependenciesCsx.Replace (info.OldMacUrl, info.NewMacUrl);
			if (info.OldVSMUrl != info.NewVSMUrl)
				info.NewDependenciesCsx = info.NewDependenciesCsx.Replace (info.OldVSMUrl, info.NewVSMUrl);

			if (info.OldDependenciesCsx != info.NewDependenciesCsx)
				return info;

			return null;
		}

		static async Task<IEnumerable<Octokit.CommitStatus>> GetLatestStatuses (GitClient client, string prefix)
		{
			try {
				var head = await client.GetHeadSha ();
				return await client.GetLatestStatuses (head, prefix);
			} catch (Exception ex) {
				Console.WriteLine ($"Could not get statuses from {client.Repository} due to exception: {ex}");
				return Enumerable.Empty<Octokit.CommitStatus> ();
			}
		}

		static string GetSha (string url)
		{
			return url == null ? null : Path.GetFileName (Path.GetDirectoryName (url));
		}

		static string GetUrl (string prefix, string fileContent)
		{
			// Find a line matching this format and extract the URL:
			// Item ("https://bosstoragemirror.azureedge.net/wrench/macios-mac-master/eb/eb35aaaa38f5cb962430ccbbec811d6b3876fc62/xamarin.mac-4.1.0.124.pkg");
			var parts = fileContent.Split (new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			var line = parts
				.Select (t => t.Trim ())
				.Where (t => t.Contains ("http://") || t.Contains ("https://"))
				.Where (t => t.Contains (prefix))
				.SingleOrDefault ();

			if (line == null)
				return null;

			parts = line.Split ('"');
			if (parts.Length != 3)
				throw new NotSupportedException ($"We expected to find a url embedded inside \"{line}\" but we found nothing!");

			// The url should be the middle part :)
			return parts [1];
		}
	}
}