// MIT License
// 
// Copyright (c) 2017 Alan McGovern
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Octokit;

namespace libgitface.ActionProviders
{
	public class ReviewPullRequestActionProvider : GitActionProvider
	{
		string[] Usernames {
			get;
		}

		public ReviewPullRequestActionProvider (GitClient client, CancellationToken token)
			: this (client, token, Enumerable.Empty<string> ())
		{

		}

		public ReviewPullRequestActionProvider (GitClient client, CancellationToken token, IEnumerable<string> usernames)
			: base (client)
		{
			if (client == null)
				throw new ArgumentNullException (nameof (client));
			if (usernames == null)
				throw new ArgumentNullException (nameof (usernames));

			Usernames = usernames.ToArray ();
		}

		protected override async Task<IAction[]> RefreshActions()
		{
			var prs = await Client.GetPullRequests ();
			var actions = new List<IAction> ();
			foreach (var pr in prs.OrderBy (p => p.Id)) {
				if (Usernames.Length > 0 && !Usernames.Contains (pr.User.Login))
					continue;

				if (pr.State == ItemState.Open) {
					actions.Add (new OpenUrlAction (Groupings.Review) {
						Url = pr.HtmlUrl.OriginalString,
						ShortDescription = $"Review {pr.Title}",
						Tooltip = $"Review pull request in {Client.Repository.Label}, created by {pr.User.Login}"
					});
				}
			}
			return actions.ToArray ();
		}
	}
}
