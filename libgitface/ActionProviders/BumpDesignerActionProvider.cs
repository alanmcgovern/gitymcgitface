using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace libgitface.ActionProviders
{
	public class BumpIncludedDesignerActionProvider : ActionProvider
	{
		static readonly Uri MDAddinsUri = new Uri ("https://github.com/xamarin/md-addins");
		static readonly Uri VisualStudioUri = new Uri ("https://github.com/xamarin/VisualStudio");

		readonly bool allowExternalActions;

		GitClient Designer {
			get;
		}

		GitClient MDAddins {
			get;
		}

		GitClient VisualStudio {
			get;
		}

		public BumpIncludedDesignerActionProvider (GitClient client, bool allowExternalActions = true)
		{
			Designer = client;
			MDAddins = client.WithRepository (new Repository (MDAddinsUri)).WithBranch (BranchMapper.ToVSMBranch (client.BranchName));
			VisualStudio = client.WithRepository (new Repository (VisualStudioUri)).WithBranch (BranchMapper.ToVisualStudioBranch (client.BranchName));
			this.allowExternalActions = allowExternalActions;
		}

		protected override async Task<IAction[]> RefreshActions()
		{
			var head = await Designer.GetHeadSha ();
			var actions = new List<IAction> ();

			// Visual Studio external repository isn't maintained anymore
			/*var statuses = (await Designer.GetLatestStatuses (head, "VSIX-")).ToArray ();
			statuses = statuses.Where (t => t.State == Octokit.CommitState.Success).ToArray ();

			foreach (var repoWithExternals in new [] { VisualStudio }) {
				// Only bump when we have 4 successful VSIX statuses
				var externalFile = await repoWithExternals.GetFileContent (".external");
				if (statuses.Length == 4 && !externalFile.Contains (head)) {
					actions.Add (new BumpExternalAction (repoWithExternals, Designer, Groupings.BumpDirect));
					actions.Add (new BumpExternalAction (repoWithExternals, Designer, Groupings.BumpPullRequest) { AllowPostActions = allowExternalActions });
				}
			}*/

			var statuses = (await Designer.GetLatestStatuses (head, "MPACK-")).ToArray ();
			statuses = statuses.Where (t => t.State == Octokit.CommitState.Success).ToArray ();

			// Only bump when we have the right number of successful MPACK statuses
			var olderBranches = new HashSet<string> {
				"release-8.0", "release-8.1"
			};
			int expectedUrlsCount = olderBranches.Contains (MDAddins.BranchName) ? 4 : 5;
			var mdaddinsFile = await MDAddins.GetFileContent ("external-addins/designer/source.txt");
			var urls = statuses.Select (t => t.TargetUrl.ToString ()).OrderBy (t => t).ToList ();

			var newFile = string.Join ("\n", urls);
			if (urls.Count == expectedUrlsCount && mdaddinsFile != newFile) {
				actions.Add (new BumpMDAddinsMPackAction (MDAddins, Designer, Groupings.BumpDirect));
				actions.Add (new BumpMDAddinsMPackAction (MDAddins, Designer, Groupings.BumpPullRequest) { AllowPostActions = allowExternalActions });
			}

			return actions.ToArray ();
		}
	}
}