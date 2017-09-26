using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace libgitface.ActionProviders
{
	public class BumpDesignerActionProvider : ActionProvider
	{
		static readonly Uri DesignerUri = new Uri ("https://github.com/xamarin/designer");
		static readonly Uri MDAddinsUri = new Uri ("https://github.com/xamarin/md-addins");
		static readonly Uri XamarinVSUri = new Uri ("https://github.com/xamarin/xamarinvs");

		GitClient Designer {
			get;
		}

		GitClient MDAddins {
			get;
		}

		GitClient XamarinVS {
			get;
		}

		public BumpDesignerActionProvider (GitClient client)
		{
			Designer = client.WithRepository (new Repository (DesignerUri));
			MDAddins = client.WithRepository (new Repository (MDAddinsUri));
			XamarinVS = client.WithRepository (new Repository (XamarinVSUri));
		}

		protected override async Task<IAction[]> RefreshActions()
		{
			var head = await Designer.GetHeadSha ();
			var actions = new List<IAction> ();

			var statuses = (await Designer.GetLatestStatuses (head, "VSIX-")).ToArray ();
			statuses = statuses.Where (t => t.State == Octokit.CommitState.Success).ToArray ();

			// Only bump when we have 4 successful VSIX statuses
			var xamarinVSFile = await XamarinVS.GetFileContent (".external");
			if (statuses.Length == 4 && !xamarinVSFile.Contains (head))
				actions.Add (new BumpExternalAction (XamarinVS, Designer, Groupings.BumpPullRequest));

			statuses = (await Designer.GetLatestStatuses (head, "MPACK-")).ToArray ();
			statuses = statuses.Where (t => t.State == Octokit.CommitState.Success).ToArray ();

			// Only bump when we have 4 successful MPACK statuses
			var mdaddinsFile = await MDAddins.GetFileContent ("external-addins/designer/source.txt");
			var newFile = string.Join ("\n", statuses.Select (t => t.TargetUrl.ToString ()).OrderBy (t => t));
			if (statuses.Length == 4 && mdaddinsFile != newFile)
				actions.Add (new BumpMDAddinsMPackAction (MDAddins, Designer, Groupings.BumpPullRequest));

			return actions.ToArray ();
		}
	}
}