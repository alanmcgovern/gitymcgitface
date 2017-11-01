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
		string AutoBumpBranchName => $"auto-bump-provisionator-{Client.BranchName}";

		public string[] Grouping { get; }
		public string ShortDescription => $"Bump provisionator ({Client.Repository.Name}/{Client.BranchName})";
		public string Tooltip => "Bumping provisionator";

		GitClient Client => Controller.Designer;

		BumpProvisionatorDependenciesController Controller {
			get;
		}

		public BumpProvisionatorDependenciesAction (BumpProvisionatorDependenciesController controller, params string[] grouping)
		{
			Controller = controller;
			Grouping = grouping;
		}

		public async void Execute()
		{
			try {
				var info = await Controller.TryBumpDependencies ();
				if (info == null)
					return;

				var title = CreateTitleText (info);
				var body = CreateBodyText (info);

				// Delete the old branch if there is one.
				if (await Client.BranchExists (AutoBumpBranchName))
					await Client.DeleteBranch (AutoBumpBranchName);

				// Update the content on a branch
				var head = await Client.GetHeadSha ();
				var client = await Client.CreateBranch (AutoBumpBranchName, head);
				await client.UpdateFileContent (title, body, Controller.ProvisionatorFile, info.NewDependenciesCsx);

				// Issue the PullRequest against the original branch
				await Client.CreatePullRequest (AutoBumpBranchName, title, body);
			} catch {

			}
		}

		string CreateTitleText (ProvisionatorInfo info)
		{
			if (info.NewIosSha != info.OldIosSha && info.NewMacSha != info.OldMacSha) {
				return $"Bump Xamarin.iOS and Xamarin.Mac";
			} else if (info.NewIosSha != info.OldIosSha) {
				return $"Bump Xamarin.iOS";
			} else if (info.NewMacSha != info.OldMacSha) {
				return $"Bump Xamarin.Mac";
			} else {
				throw new NotSupportedException ("Can't generate the title for this bump");
			}
		}

		string CreateBodyText (ProvisionatorInfo info)
		{
			var lines = new List<string> ();
			if (info.NewIosSha != info.OldIosSha)
				lines.Add ($"Xamarin.iOS: {Controller.MacIos.Repository.Uri}/compare/{info.OldIosSha}...{info.NewIosSha}");
			if (info.NewMacSha != info.OldMacSha)
				lines.Add ($"Xamarin.Mac: {Controller.MacIos.Repository.Uri}/compare/{info.OldMacSha}...{info.NewMacSha}");
			return string.Join ("\r\n", lines);
		}
	}
}
