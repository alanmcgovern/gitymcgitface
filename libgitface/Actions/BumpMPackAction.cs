using System;
using System.IO;
using System.Linq;

namespace libgitface
{
	public class BumpMDAddinsMPackAction : IAction
	{
		string AutoBumpBranchName => $"{MDAddinsClient.BranchName}-bump-designer";
		public string ShortDescription => $"Bump included designer ({MDAddinsClient.Repository.Name}/{MDAddinsClient.BranchName})";
		public string[] Grouping { get; }
		public string Tooltip => $"Bump {DesignerExternal.Repository.Label} reference inside {MDAddinsClient.Repository.Label}";

		GitClient MDAddinsClient { get; }
		GitClient DesignerExternal { get; }

		bool UsePullRequest {
			get { return Grouping.Contains (Groupings.PR); }
		}

		public BumpMDAddinsMPackAction (GitClient mdaddinsClient, GitClient designerExternal, params string[] grouping)
		{
			MDAddinsClient = mdaddinsClient;
			DesignerExternal = designerExternal;
			Grouping = grouping;
		}

		public async void Execute()
		{
			var designerHeadSha = await DesignerExternal.GetHeadSha ();
			var statuses = (await DesignerExternal.GetLatestStatuses (designerHeadSha, "MPACK-")).ToArray ();
			statuses = statuses.Where (t => t.State == Octokit.CommitState.Success).ToArray ();
			// Only bump if we have 4 successful MPACK statuses
			if (statuses.Length != 4)
				return;

			var currentFile = await MDAddinsClient.GetFileContent ("external-addins/designer/source.txt");
			var newFile = string.Join ("\n", statuses.Select (t => t.TargetUrl.ToString ()).OrderBy (t => t));

			var uri = new Uri (currentFile.Split (new [] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).First ());
			var designerCurrentSha = Path.GetFileName (Path.GetDirectoryName (uri.PathAndQuery));

			if (UsePullRequest && await MDAddinsClient.BranchExists (AutoBumpBranchName))
				await MDAddinsClient.DeleteBranch (AutoBumpBranchName);

			var title = $"[{MDAddinsClient.BranchName}] Bump {DesignerExternal.Repository.Label}";
			var body = $"{DesignerExternal.Repository.Uri}/compare/{designerCurrentSha}...{designerHeadSha}";

			// Update the content on a branch
			var head = await MDAddinsClient.GetHeadSha ();

			var client = MDAddinsClient;
			if (UsePullRequest)
				client = await MDAddinsClient.CreateBranch (AutoBumpBranchName, head);

			await client.UpdateFileContent (title, body, "external-addins/designer/source.txt", newFile);

			// Issue the PullRequest against the original branch
			if (UsePullRequest)
				await MDAddinsClient.CreatePullRequest (AutoBumpBranchName, title, body);
		}
	}
}
