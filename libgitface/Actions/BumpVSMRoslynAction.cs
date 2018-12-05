using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Octokit;

namespace libgitface
{
	public class BumpVSMRoslynAction : IAction
	{
		public string[] Grouping { get; }
		public string ShortDescription { get; }
		public string Tooltip { get; }

		public bool AllowPostActions { get; set; }

		BumpVSMRoslynController Controller {
			get;
		}

		public BumpVSMRoslynAction (BumpVSMRoslynController controller, params string[] grouping)
		{
			Controller = controller;
			Grouping = grouping;
			ShortDescription = $"Bump VSM Roslyn nuget ({Controller.Designer.Repository.Name}/{Controller.Designer.BranchName})";
			Tooltip = ShortDescription;
		}

		public async void Execute()
		{
			try {
				await Controller.UpdateDesignerRoslynVersion ();
			} catch {

			}
		}
	}
}
