﻿using System;
using System.IO;
using System.Linq;

namespace libgitface
{
	public class BumpMDAddinsMPackAction : IAction
	{
		string AutoBumpBranchName => $"auto-bump-designer-external-{MDAddinsClient.BranchName}";
		public string ShortDescription => $"Bump included designer ({MDAddinsClient.Repository.Name}/{MDAddinsClient.BranchName})";
		public string[] Grouping { get; }
		public string Tooltip => $"Bump {DesignerExternal.Repository.Label} reference inside {MDAddinsClient.Repository.Label}";

		GitClient MDAddinsClient { get; }
		GitClient DesignerExternal { get; }
		public BumpMDAddinsMPackAction (GitClient mdaddinsClient, GitClient designerExternal, params string[] grouping)
		{
			MDAddinsClient = mdaddinsClient;
			DesignerExternal = designerExternal;
			Grouping = grouping;
		}

		public async void Execute()
		{
			var designerHeadSha = await DesignerExternal.GetHeadSha ();
			var statuses = await DesignerExternal.GetLatestStatuses (designerHeadSha, "MPACK-");
			var currentFile = await MDAddinsClient.GetFileContent ("external-addins/designer/designer/source.txt");
			var newFile = string.Join ("\n", statuses.OrderBy (t => t.TargetUrl).Select (t => t.TargetUrl.ToString ()));

			var uri = new Uri (currentFile.Split (new [] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).First ());
			var designerCurrentSha = Path.GetFileName (Path.GetDirectoryName (uri.PathAndQuery));

			if (await MDAddinsClient.BranchExists (AutoBumpBranchName))
				await MDAddinsClient.DeleteBranch (AutoBumpBranchName);

			var title = $"Bump {DesignerExternal.Repository.Label}";
			var body = $"{DesignerExternal.Repository.Uri}/compare/{designerCurrentSha}...{designerHeadSha}";

			// Update the content on a branch
			var head = await MDAddinsClient.GetHeadSha ();
			var client = await MDAddinsClient.CreateBranch (AutoBumpBranchName, head);
			await client.UpdateFileContent (title, body, "external-addins/designer/designer/source.txt", newFile);

			// Issue the PullRequest against the original branch
			await MDAddinsClient.CreatePullRequest (AutoBumpBranchName, title, body);
		}
	}
}