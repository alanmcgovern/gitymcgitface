using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace libgitface
{
	public class Dependency
	{
		public Uri GitHubUrl { get; }
		public string GitSha { get; }
		public string InstallerPrefix { get;  }
		public string InstallerUrl { get;  }
		public string Name { get; }
		public string Prefix { get; }

		public Dependency (string name, string prefix, Uri githubUrl, string installerPrefix)
			: this (name, prefix, githubUrl, installerPrefix, null, null)
		{
		}

		public Dependency (string name, string prefix, Uri githubUrl, string installerPrefix, string installerUrl, string gitSha)
		{
			Name = name;
			Prefix = prefix;
			GitHubUrl = githubUrl;
			InstallerPrefix = installerPrefix;
			InstallerUrl = installerUrl;
			GitSha = gitSha;
		}

		public Dependency WithUrl (string installerUrl)
		{
			var gitSha = Path.GetFileName (Path.GetDirectoryName (installerUrl));
			return new Dependency (Name, Prefix, GitHubUrl, InstallerPrefix, installerUrl, gitSha);
		}
	}

	public class ProvisionatorProfile {
		public string Content { get; }
		public IReadOnlyList<Dependency> Dependencies { get; }

		public ProvisionatorProfile (string content, IEnumerable<Dependency> dependencies)
		{
			Content = content;
			Dependencies = dependencies.OrderBy (t => t.Name).ToArray ();
		}

		public ProvisionatorProfile WithDependency (Dependency dependency)
		{
			var deps = new List<Dependency> (Dependencies);
			var oldDep = deps.First (t => t.Name == dependency.Name);
			deps.Remove (oldDep);
			deps.Add (dependency);

			var content = Content.Replace (oldDep.InstallerUrl, dependency.InstallerUrl);
			return new ProvisionatorProfile (content, deps);
		}
	}

	public class ProvisionatorProfileParser
	{
		/// <summary>
		/// Name/Identifier pairs to match the product name with the name embedded in the installer files
		/// </summary>
		static readonly Dependency[] KnownDependencies = new [] {
			new Dependency ("Mono",              "MonoFramework",      new Uri ("https://github.com/mono/mono"),              "PKG-"),
			new Dependency ("Xamarin.Android",   "xamarin.android",    new Uri ("https://github.com/xamarin/monodroid"),      "PKG-"),
			new Dependency ("Xamarin.iOS",       "xamarin.ios",        new Uri ("https://github.com/xamarin/xamarin-macios"), "PKG-"),
			new Dependency ("Xamarin.Mac",       "xamarin.mac",        new Uri ("https://github.com/xamarin/xamarin-macios"), "PKG-"),
			new Dependency ("Visual Studio Mac", "VisualStudioForMac", new Uri ("https://github.com/mono/monodevelop"),       "DMG-"),
		};

		public static ProvisionatorProfile Parse (string content)
		{
			var dependencies = new List<Dependency> ();
			foreach (var dependency in KnownDependencies) {
				var url = GetUrl (dependency.Prefix, content);
				if (url != null) {
					dependencies.Add (dependency.WithUrl (url));
				}
			}
			return new ProvisionatorProfile (content, dependencies);
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

	public class BumpProvisionatorDependenciesController
	{
		public GitClient Designer {
			get;
		}

		public string ProvisionatorFile => "bot-provisioning/dependencies.csx";

		public BumpProvisionatorDependenciesController (GitClient client)
		{
			Designer = client;
		}

		/// <summary>
		/// Returns null if no dependency needs updating, otherwise returns the new content for the `bot-provisioning/dependencies.csx` file.
		/// </summary>
		/// <returns>The bump dependencies.</returns>
		public async Task<Tuple<ProvisionatorProfile, ProvisionatorProfile>> TryBumpDependencies ()
		{
			var profileContent = await Designer.GetFileContent (ProvisionatorFile);
			var profile = ProvisionatorProfileParser.Parse (profileContent);

			var statuses = Enumerable.Empty<Octokit.CommitStatus> ();
			var githubRepos = profile.Dependencies.Distinct (t => t.GitHubUrl).ToArray ();
			foreach (var dependency in githubRepos) {
				var client = Designer.WithRepository (new Repository (dependency.GitHubUrl));
				if (dependency.Name == "Mono")
					client = client.WithBranch (BranchMapper.ToMonoBranch (client.BranchName));
				if (dependency.Name == "Visual Studio Mac")
					client = client.WithBranch (BranchMapper.ToVSMBranch (client.BranchName));

				statuses = statuses.Concat (await GetLatestStatuses (client, dependency.InstallerPrefix));
			}

			var newUrls = statuses
				.Select (t => t.TargetUrl.ToString ())
				.ToArray ();

			var newProfile = profile;
			foreach (var dependency in profile.Dependencies) {
				var newDependency = dependency.WithUrl (newUrls.Where (t => t.Contains (dependency.Prefix)).FirstOrDefault () ?? dependency.InstallerUrl);
				newProfile = newProfile.WithDependency (newDependency);
			}
			if (profile.Content == newProfile.Content)
				return null;
			return Tuple.Create (profile, newProfile);
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

	}
}