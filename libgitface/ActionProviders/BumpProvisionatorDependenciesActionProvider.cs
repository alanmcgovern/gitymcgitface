using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace libgitface.ActionProviders
{
	public class BumpProvisionatorDependenciesActionProvider : ActionProvider
	{
		readonly bool allowExternalActions;

		GitClient Client {
			get;
		}

		public BumpProvisionatorDependenciesActionProvider (GitClient client, bool allowExternalActions = true)
		{
			Client = client;
			this.allowExternalActions = allowExternalActions;
		}

		protected override async Task<IAction[]> RefreshActions()
		{
			var newDependencies = await new BumpProvisionatorDependenciesController (Client).TryBumpDependencies ();
			var actions = new List<IAction> ();

			if (newDependencies != null)
				actions.Add (new BumpProvisionatorDependenciesAction (new BumpProvisionatorDependenciesController (Client), newDependencies, Groupings.BumpPullRequest) { AllowPostActions = allowExternalActions });
			return actions.ToArray ();
		}
	}
}