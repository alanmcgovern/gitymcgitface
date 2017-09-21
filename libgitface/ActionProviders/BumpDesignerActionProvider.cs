using System;
namespace libgitface.ActionProviders
{
	public class BumpDesignerActionProvider : GitActionProvider
	{
		public BumpDesignerActionProvider (GitClient client)
			: base (client)
		{
		}

		protected override System.Threading.Tasks.Task<IAction[]> RefreshActions()
		{
			return base.RefreshActions();
		}
	}
}