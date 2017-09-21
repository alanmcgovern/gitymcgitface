using System;
namespace libgitface
{
	public sealed class SubmoduleInformation
	{
		public string CurrentSha { get; }
		public string HeadSha { get; }
		public string Path { get; }
		public Repository Repository { get; }

		public SubmoduleInformation (Repository repository, string path, string currentSha, string headSha)
		{
			Repository = repository;
			Path = path;
			CurrentSha = currentSha;
			HeadSha = headSha;
		}

		public override string ToString()
		{
			return string.Format("[SubmoduleInformation: CurrentSha={0}, HeadSha={1}, Path={2}]", CurrentSha, HeadSha, Path);
		}
	}
}
