using System;
using System.Threading.Tasks;
using System.Diagnostics;

namespace libgitface
{
	public static class GitClientExtensions
	{
		public static async Task CreateAndOpenPullRequest (this GitClient client, string branchName, string titleText, string bodyText, bool openPrInBrowser = true)
		{
			var prUrl = await client.CreatePullRequest (branchName, titleText, bodyText);
			if (openPrInBrowser)
				new OpenUrlAction { Url = prUrl }.Execute ();
		}
	}
}
