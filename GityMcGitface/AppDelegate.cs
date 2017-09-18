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
using System.Threading;

using AppKit;
using Foundation;

using Octokit;
using Octokit.Reactive;

using libgitface.ActionProviders;

namespace GityMcGitface
{
	[Register("AppDelegate")]
	public class AppDelegate : NSApplicationDelegate
	{
		ApplicationIconActionCenter ActionCentre {
			get;
		}

		ProductHeaderValue Product => new ProductHeaderValue ("gity-mc-gitface");

		libgitface.Repository[] Repositories => new [] {
			//new libgitface.Repository (new Uri ("https://github.com/alanmcgovern/gitymcgitface")),
			new libgitface.Repository (new Uri ("https://github.com/alanmcgovern/designer"))
		};

		string[] Usernames => new [] {
			"alanmcgovern",
		};

		public AppDelegate()
		{
			NSStatusBar bar = NSStatusBar.SystemStatusBar;
			NSStatusItem statusItem = bar.CreateStatusItem (NSStatusItemLength.Square);
			ActionCentre = new ApplicationIconActionCenter (statusItem, SynchronizationContext.Current);
		}

		public override void DidFinishLaunching(NSNotification notification)
		{
			Func<GitHubClient> clientFactory = () => new GitHubClient (Product) {
				Credentials = new Credentials (Secrets.GithubToken)
			};

			foreach (var repository in Repositories) {
				ActionCentre.ActionProviders.Add (new ReviewPullRequestActionProvider (clientFactory, repository, CancellationToken.None, Usernames));
				ActionCentre.ActionProviders.Add (new SubmoduleAnalyzer (clientFactory, repository, "master"));
			}
			ActionCentre.Refresh ();
		}

		public override void WillTerminate(NSNotification notification)
		{
			// Insert code here to tear down your application
		}
	}
}
