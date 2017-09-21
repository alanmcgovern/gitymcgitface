using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Octokit;

namespace libgitface.ActionProviders
{
	public class SubmoduleAnalyzer : GitActionProvider
	{
		public SubmoduleAnalyzer (GitClient client)
			: base (client)
		{

		}

		public override async void Refresh ()
		{
			try {
				var parser = new GitModulesParser ();
				var gitModulesContent = await Client.GetFileContent (".gitmodules");
				var modules = parser.Parse (gitModulesContent);

				var infos = await Task.WhenAll (modules.Select (GetCurrentHashAndBranchTip));
				var requiresUpdate = infos.Where (t => t.HeadSha != t.CurrentSha).ToArray ();
				Actions = new [] { new CreateSubmoduleBumpPRAction (Client, requiresUpdate) };
			} catch (Octokit.NotFoundException) {
				// This does not have submodules
			} catch (Exception ex) {
				Log.Exception ("SubmoduleAnalyzer failed", ex);
			}
		}

		async Task<SubmoduleInformation> GetCurrentHashAndBranchTip (SubmoduleEntry entry)
		{
			var submodule = Client
				.WithRepository (new Repository (Client.Repository.Uri, entry.Url))
				.WithBranch (entry.Branch);

			var currentSubmoduleSha = await Client.GetSubmoduleSha (entry);
			var headShaOfTrackedBranch = await submodule.GetHeadSha ();
			return new SubmoduleInformation (submodule.Repository, entry.Path, currentSubmoduleSha, headShaOfTrackedBranch);
		}
	}
}
