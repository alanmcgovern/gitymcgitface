using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Octokit;

namespace libgitface.ActionProviders
{
	public class SubmoduleAnalyzer : GitHubActionProvider
	{
		public string Branch {
			get;
		}

		public SubmoduleAnalyzer (Func<GitHubClient> client, Repository repository, string branch)
			: base (client, repository)
		{
			Branch = string.IsNullOrEmpty (branch) ? "master" : branch;
		}

		public override async void Refresh ()
		{
			try {
				var parser = new GitModulesParser ();
				var gitModulesContent = await GetFileContent (".gitmodules", Branch);
				var modules = parser.Parse (gitModulesContent);

				var infos = await Task.WhenAll (modules.Select (GetCurrentHashAndBranchTip));
				var requiresUpdate =  infos.Where (t => t.CurrentHash != t.BranchTip).ToArray ();
				Actions = requiresUpdate.Select (info => new BumpSubmoduleAction (CreateClient (), Repository, Branch, info)).ToArray ();
			} catch (Octokit.NotFoundException) {
				// This does not have submodules
			} catch (Exception ex) {
				Log.Exception ("SubmoduleAnalyzer failed", ex);
			}
		}

		async Task<SubmoduleInformation> GetCurrentHashAndBranchTip (SubmoduleEntry entry)
		{
			var submoduleRepository = new Repository (Repository.Uri, entry.Url);
			var currentSubmoduleHashTask = CreateClient ().Repository.Content.GetAllContentsByRef (Repository.Owner, Repository.Name, entry.Path, Branch);
			var headCommitOfRepositoryTask = CreateClient ().Git.Tree.Get (submoduleRepository.Owner, submoduleRepository.Name, entry.Branch);

			var currentSubmoduleHash = await currentSubmoduleHashTask.ConfigureAwait (false);
			var headCommitOfRepository = await headCommitOfRepositoryTask.ConfigureAwait (false);
			return new SubmoduleInformation (submoduleRepository, entry.Path, currentSubmoduleHash [0].Sha, headCommitOfRepository.Sha);
		}
	}
}
