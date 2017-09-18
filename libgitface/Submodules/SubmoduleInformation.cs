using System;
namespace libgitface
{
	public sealed class SubmoduleInformation
	{
		public string BranchTip {
			get;
		}

		public string CurrentHash {
			get;
		}

		public string Path {
			get;
		}

		public Repository Repository {
			get;
		}

		public SubmoduleInformation (Repository repository, string path, string currentHash, string branchTip)
		{
			Repository = repository;
			Path = path;
			CurrentHash = currentHash;
			BranchTip = branchTip;
		}

		public override string ToString()
		{
			return string.Format("[SubmoduleInformation: BranchTip={0}, CurrentHash={1}, Path={2}]", BranchTip, CurrentHash, Path);
		}
	}
}
