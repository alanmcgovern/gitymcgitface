using System;
using System.Diagnostics;

namespace libgitface
{
	public class BumpSubmoduleAction : IAction
	{
		string ComparisonUri {
			get { return $"{Submodule.Uri}/compare/{CurrentSha}...{NewSha}"; }
		}

		SubmoduleInformation Info {
			get;
		}

		public Repository MainRepository {
			get;
		}

		public Repository Submodule {
			get { return Info.Repository; }
		}

		public string CurrentSha {
			get { return Info.CurrentHash; }
		}

		public string NewSha {
			get { return Info.BranchTip; }
		}

		public string ShortDescription => $"{Submodule.Owner}/{Submodule.Name} -> {MainRepository.Owner}/{MainRepository.Name}";

		public string Tooltip => "Bump the submodule";

		public BumpSubmoduleAction (Repository mainRepository, SubmoduleInformation info)
		{
			MainRepository = mainRepository;
			Info = info;
		}

		public void Execute()
		{
			Process.Start (ComparisonUri);
		}
	}
}
