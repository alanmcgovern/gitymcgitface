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
		string AutoBumpBranchName => $"{Client.BranchName}-bump-provisionator";

		public string[] Grouping { get; }
		public string ShortDescription { get; }
		public string Tooltip { get; }

		GitClient Client => Controller.Designer;

		BumpProvisionatorDependenciesController Controller {
			get;
		}

		public BumpProvisionatorDependenciesAction (BumpProvisionatorDependenciesController controller, ProvisionatorInfo info, params string[] grouping)
		{
			Controller = controller;
			Grouping = grouping;
			ShortDescription = $"Bump provisionator ({Client.Repository.Name}/{Client.BranchName})";
			Tooltip = CreateTitleText (info);
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
			var products = new List<string> ();
			if (info.NewAndroidSha != info.OldAndroidSha)
				products.Add ("Xamarin.Android");
			if (info.NewIosSha != info.OldIosSha)
				products.Add ("Xamarin.iOS");
			if (info.NewMacSha != info.OldMacSha)
				products.Add ("Xamarin.Mac");
			if (info.NewVSMSha != info.OldVSMSha)
				products.Add ("VisualStudioMac");
			return $"[{Client.BranchName}] Bump {string.Join (", ", products)}.";
		}

		string CreateBodyText (ProvisionatorInfo info)
		{
			var lines = new List<string> ();
			if (info.NewAndroidSha != info.OldAndroidSha)
				lines.Add ($"Xamarin.Android: {Controller.MonoDroid.Repository.Uri}/compare/{info.OldAndroidSha}...{info.NewAndroidSha}");
			if (info.NewIosSha != info.OldIosSha)
				lines.Add ($"Xamarin.iOS: {Controller.MacIos.Repository.Uri}/compare/{info.OldIosSha}...{info.NewIosSha}");
			if (info.NewMacSha != info.OldMacSha)
				lines.Add ($"Xamarin.Mac: {Controller.MacIos.Repository.Uri}/compare/{info.OldMacSha}...{info.NewMacSha}");
			if (info.NewVSMSha != info.OldVSMSha)
				lines.Add ($"Visual Studio Mac: {Controller.VisualStudioMac.Repository.Uri}/compare/{info.OldVSMSha}...{info.NewVSMSha}");
			return string.Join ("\r\n", lines);
		}
	}
}
