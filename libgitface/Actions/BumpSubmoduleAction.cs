using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Octokit;

namespace libgitface
{
	public class BumpSubmoduleAction : IAction
	{
		GitHubClient Client {
			get;
		}

		string ComparisonUri {
			get { return $"{Submodule.Uri}/compare/{CurrentSha}...{NewSha}"; }
		}

		SubmoduleInformation Info {
			get;
		}

		public Repository MainRepository {
			get;
		}

		public string MainRepositoryBranch {
			get;
		}

		public Repository Submodule {
			get { return Info.Repository; }
		}

		public string CurrentSha {
			get { return Info.CurrentHash; }
		}

		public string NewSha {
			get { return Info.BranchTip; }
		}

		public string ShortDescription => $"{Submodule.Owner}/{Submodule.Name} -> {MainRepository.Owner}/{MainRepository.Name}";

		public string Tooltip => "Bump the submodule";

		public BumpSubmoduleAction (GitHubClient client, Repository mainRepository, string mainRepositoryBranch, SubmoduleInformation info)
		{
			Client = client;
			MainRepository = mainRepository;
			MainRepositoryBranch = mainRepositoryBranch;
			Info = info;
		}

		public async void Execute()
		{
			try {
				await CreateSubmoduleUpdateCommit ();
			} catch {

			}
		}

		async Task<string> CreateSubmoduleUpdateCommit ()
		{
			string commitMessage = $@"Bump submodule {Submodule.Owner}/{Submodule.Name}

{ComparisonUri}";

			var parentBranchLatestSha = (await Client.Git.Tree.Get (MainRepository.Owner, MainRepository.Name, MainRepositoryBranch)).Sha;
			var updateParentTree = new NewTree { BaseTree = parentBranchLatestSha };
			updateParentTree.Tree.Add(new NewTreeItem {
				Mode = FileMode.Submodule,
				Sha = NewSha,
				Path = Info.Path,
				Type = TreeType.Commit,
			});

			var newParentTree = await Client.Git.Tree.Create (MainRepository.Owner, MainRepository.Name, updateParentTree);
			var newCommit = new NewCommit (commitMessage, newParentTree.Sha, parentBranchLatestSha);
			var pullRequestBranchRef = $"heads/bump-{Submodule.Owner}-{Submodule.Name}";
			var commit = await Client.Git.Commit.Create (MainRepository.Owner, MainRepository.Name, newCommit);
			Console.WriteLine($"Updating submodule with commit on fork branch: {commit.Sha}.");
			await Client.Git.Reference.Create (MainRepository.Owner, MainRepository.Name, new NewReference ("refs/" + pullRequestBranchRef, parentBranchLatestSha));
			await Client.Git.Reference.Update (MainRepository.Owner, MainRepository.Name, pullRequestBranchRef, new ReferenceUpdate(commit.Sha));
			return pullRequestBranchRef;
		}

	}
}
