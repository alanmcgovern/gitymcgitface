using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using libgitface.Actions;

namespace libgitface.ActionProviders
{
	public class VSMBuddyTestActionProvider : GitActionProvider
	{
		IDownloader Downloader { get; }

		public VSMBuddyTestActionProvider (GitClient client, IDownloader downloader)
			: base(client)
		{
			Downloader = downloader;
		}

		protected override async Task<IAction[]> RefreshActions()
		{
			var pulls = await Client.GetPullRequests ();
			List<IAction> results = new List<IAction> ();
			foreach (var pull in pulls) {
				var statuses = await Client.GetLatestStatuses (pull.Head.Sha, "MPACK-");
				if (statuses.Count == 4 && statuses.All (s => s.State == Octokit.CommitState.Success))
					results.Add (new VSMBuddyTestAction (Client, pull, Downloader, Groupings.BuddyTest));
			}

			return results.ToArray ();
		}
	}
}
