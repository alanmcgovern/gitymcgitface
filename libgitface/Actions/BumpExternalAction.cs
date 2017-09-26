using System;
using System.Linq;
using Octokit;

namespace libgitface
{
	public class BumpExternalAction : IAction
	{
		string AutoBumpBranchName => $"auto-bump-designer-external-{Client.BranchName}";
		public string[] Grouping { get; }
		public string ShortDescription => $"Bump included designer ({Client.Repository.Name}/{Client.BranchName})";
		public string Tooltip => $"Bump {External.Repository.Label} reference inside {Client.Repository.Label}";

		GitClient Client {
			get;
		}

		GitClient External {
			get;
		}

		public BumpExternalAction (GitClient client, GitClient external, params string[] grouping)
		{
			Client = client;
			External = external;
			Grouping = grouping;
		}


		public async void Execute()
		{
			string externalOldSha;
			var externalHead = await External.GetHeadSha ();
			var externalFile = await Client.GetFileContent (".external");
			externalFile = UpdateExternal (externalFile, externalHead, out externalOldSha);

			if (await Client.BranchExists (AutoBumpBranchName))
				await Client.DeleteBranch (AutoBumpBranchName);

			var title = $"Bump {External.Repository.Label}";
			var body = $"{External.Repository.Uri}/compare/{externalOldSha}...{externalHead}";

			// Update the content on a branch
			var head = await Client.GetHeadSha ();
			var client = await Client.CreateBranch (AutoBumpBranchName, head);
			await client.UpdateFileContent (title, body, ".external", externalFile);

			// Issue the PullRequest against the original branch
			await Client.CreatePullRequest (AutoBumpBranchName, title, body); 
		}

		string UpdateExternal (string content, string newSha, out string oldSha)
		{
			bool useCRLF = content.IndexOf ("\r\n") > 0;
			var lines = content.Split (new [] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

			var designerLabel = External.Repository.Label;
			oldSha = null;
			for (int i = 0; i < lines.Length; i ++) {
				if (lines [i].StartsWith (designerLabel)) {
					oldSha = lines [i].Split ('@').Last ();
					lines [i] = $"{designerLabel}:{External.BranchName}@{newSha}";
					break;
				}
			}
			return string.Join (useCRLF ? "\r\n" : "\n", lines);
		}

	}
}
