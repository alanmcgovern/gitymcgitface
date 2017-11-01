using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace libgitface.ActionProviders
{
	public class BumpProvisionatorDependenciesActionProvider : ActionProvider
	{
		BumpProvisionatorDependenciesController Controller {
			get;
		}

		public BumpProvisionatorDependenciesActionProvider (GitClient client)
		{
			Controller = new BumpProvisionatorDependenciesController (client);
		}

		protected override async Task<IAction[]> RefreshActions()
		{
			var newDependencies = await Controller.TryBumpDependencies ();
			var actions = new List<IAction> ();

			if (newDependencies != null)
				actions.Add (new BumpProvisionatorDependenciesAction (Controller, newDependencies, Groupings.BumpPullRequest));
			return actions.ToArray ();
		}
	}
}