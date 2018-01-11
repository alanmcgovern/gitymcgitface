using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace libgitface.ActionProviders
{
	public class BumpVSMRoslynActionProvider : ActionProvider
	{
		BumpVSMRoslynController Controller {
			get;
		}

		public BumpVSMRoslynActionProvider (GitClient client)
		{
			Controller = new BumpVSMRoslynController (client);
		}

		protected override async Task<IAction[]> RefreshActions()
		{
			if (await Controller.NeedsUpdate ())
				return new [] { new BumpVSMRoslynAction (Controller, Groupings.BumpPullRequest) };
			return Array.Empty<IAction> ();
		}
	}
}