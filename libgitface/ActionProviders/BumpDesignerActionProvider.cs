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
			//var mdaddinsFile = await MDAddins.GetFileContent ("external-addins/designer/pull-mpack.sh");
			var xamarinVSFile = await XamarinVS.GetFileContent (".external");

			var actions = new List<IAction> ();
			if (!xamarinVSFile.Contains (head))
				actions.Add (new BumpExternalAction (XamarinVS, Designer, Groupings.BumpPullRequest));

			return actions.ToArray ();
		}
	}
}