using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Octokit;

namespace libgitface
{
	public class CreateSubmoduleBumpPRAction : IAction
	{
		string AutoBumpBranchName => $"auto-bump-submodules-{Client.BranchName}";
		GitClient Client { get; }
		SubmoduleInformation[] Submodules { get; }

		public string[] Grouping { get; }
		public string ShortDescription => $"Bump submodules ({Client.Repository.Name}/{Client.BranchName})";
		public string Tooltip => CreateTitleText ();

		public CreateSubmoduleBumpPRAction (GitClient client, SubmoduleInformation[] submodules, params string[] grouping)
		{
			Client = client;
			Submodules = submodules;
			Grouping = grouping;
		}

		public async void Execute()
		{
			try {
				var branchName = await CreateBranchWithBump ();
				await Client.CreatePullRequest (branchName, CreateTitleText (), CreateBodyText ());
			} catch {

			}
		}

		async Task<string> CreateBranchWithBump ()
		{
			var headSha = await Client.GetHeadSha ();
			var tree = new NewTree { BaseTree = headSha };
			foreach (var submodule in Submodules) {
				tree.Tree.Add(new NewTreeItem {
					Mode = FileMode.Submodule,
					Sha = submodule.HeadSha,
					Path = submodule.Path,
					Type = TreeType.Commit,
				});
			}

			var newTreeSha = await Client.CreateTree (tree);
			var commitSha = await Client.CreateCommit (CreateTitleText () + "\r\n\r\n" + CreateBodyText (), newTreeSha, headSha);
			if (await Client.BranchExists (AutoBumpBranchName))
				await Client.DeleteBranch (AutoBumpBranchName);
			await Client.CreateBranch (AutoBumpBranchName, commitSha);
			return AutoBumpBranchName;
		}

		string CreateTitleText ()
		{
			var submoduleNames = string.Join (", ", Submodules.Select (t => t.Repository.Name));
			return $"Bumping {submoduleNames}";
		}

		string CreateBodyText ()
		{
			var urls = Submodules.Select (s => $"{s.Repository.Uri}/compare/{s.CurrentSha}...{s.HeadSha}");
			return string.Join ("\r\n", urls);
		}
	}
}
