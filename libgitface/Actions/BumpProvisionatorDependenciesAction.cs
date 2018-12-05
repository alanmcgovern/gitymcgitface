using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Octokit;

namespace libgitface
{
	public class BumpProvisionatorDependenciesAction : IAction
	{
		string AutoBumpBranchName => $"{Client.BranchName}-bump-provisionator";

		public string[] Grouping { get; }
		public string ShortDescription { get; }
		public string Tooltip { get; }

		public bool AllowPostActions { get; set; }

		GitClient Client => Controller.Designer;

		BumpProvisionatorDependenciesController Controller {
			get;
		}

		public BumpProvisionatorDependenciesAction (BumpProvisionatorDependenciesController controller, Tuple<ProvisionatorProfile, ProvisionatorProfile> info, params string[] grouping)
		{
			Controller = controller;
			Grouping = grouping;
			ShortDescription = $"Bump provisionator ({Client.Repository.Name}/{Client.BranchName})";
			Tooltip = CreateTitleText (info.Item1, info.Item2);
		}

		public async void Execute()
		{
			try {
				var info = await Controller.TryBumpDependencies ();
				if (info == null)
					return;

				var title = CreateTitleText (info.Item1, info.Item2);
				var body = CreateBodyText (info.Item1, info.Item2);

				// Delete the old branch if there is one.
				if (await Client.BranchExists (AutoBumpBranchName))
					await Client.DeleteBranch (AutoBumpBranchName);

				// Update the content on a branch
				var head = await Client.GetHeadSha ();
				var client = await Client.CreateBranch (AutoBumpBranchName, head);
				await client.UpdateFileContent (title, body, Controller.ProvisionatorFile, info.Item2.Content);

				// Issue the PullRequest against the original branch
				await Client.CreatePullRequest (AutoBumpBranchName, title, body);
			} catch {

			}
		}

		string CreateTitleText (ProvisionatorProfile oldProfile, ProvisionatorProfile newProfile)
		{
			var products = new List<string> ();
			for (int i = 0; i < oldProfile.Dependencies.Count; i ++) {
				if (oldProfile.Dependencies [i].GitSha != newProfile.Dependencies [i].GitSha)
					products.Add (oldProfile.Dependencies [i].Name);
			}
			return $"[{Client.BranchName}] Bump {string.Join (", ", products)}.";
		}

		string CreateBodyText (ProvisionatorProfile oldProfile, ProvisionatorProfile newProfile)
		{
			var lines = new List<string> ();
			for (int i = 0; i < oldProfile.Dependencies.Count; i ++) {
				var oldDep = oldProfile.Dependencies [i];
				var newDep = newProfile.Dependencies [i];
				if (newDep.GitSha != oldDep.GitSha)
					lines.Add ($"{oldDep.Name}: {oldDep.GitHubUrl}/compare/{oldDep.GitSha}...{newDep.GitSha}");
			}
			return string.Join ("\r\n", lines);
		}
	}
}
