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

using libgitface.ActionProviders;
using libgitface;

namespace GityMcGitface
{
	[Register("AppDelegate")]
	public class AppDelegate : NSApplicationDelegate
	{
		static readonly libgitface.Repository DesignerRepository = new libgitface.Repository (new Uri ("https://github.com/xamarin/designer"));
		static readonly libgitface.Repository SimulatorRepository = new libgitface.Repository (new Uri ("https://github.com/xamarin/ios-sim-sharp"));

		ApplicationIconActionCenter ActionCentre {
			get;
		}

		public string[] Branches => new string[] {
			"d16-0",
			"d15-9",
			"master",
		};

		ProductHeaderValue Product => new ProductHeaderValue ("gity-mc-gitface");

		libgitface.Repository[] Repositories { get; } = new [] {
			DesignerRepository,
			//SimulatorRepository,
		};

		public AppDelegate()
		{
			NSStatusBar bar = NSStatusBar.SystemStatusBar;
			NSStatusItem statusItem = bar.CreateStatusItem (NSStatusItemLength.Square);
			ActionCentre = new ApplicationIconActionCenter (statusItem, SynchronizationContext.Current);
		}

		public override void DidFinishLaunching(NSNotification notification)
		{
			Func<GitHubClient> gitHubClientFactory = () => new GitHubClient (Product) {
				Credentials = new Credentials (Secrets.GithubToken)
			};

			var baseClient = new GitClient (gitHubClientFactory);

			// For each tracked repository we should check the submodule status for each tracked branch.
			foreach (var repository in Repositories) {
				foreach (var branch in Branches) {
					ActionCentre.ActionProviders.Add (new SubmoduleAnalyzer (baseClient.WithBranch (branch).WithRepository (repository)));
					ActionCentre.ActionProviders.Add (new BumpProvisionatorDependenciesActionProvider (baseClient.WithBranch (branch).WithRepository (repository)));
				}
			}

			// For each tracked branch we should make sure the latest designer commit is integrated with VS and XS.
			foreach (var branch in Branches) {
				ActionCentre.ActionProviders.Add (new BumpIncludedDesignerActionProvider (baseClient.WithBranch (branch).WithRepository (DesignerRepository)));
				ActionCentre.ActionProviders.Add (new BumpVSMRoslynActionProvider (baseClient.WithBranch (branch).WithRepository (DesignerRepository)));
			}

			// For each tracked repository we should keep tabs on open PRs
			//foreach (var repository in Repositories) {
			//	ActionCentre.ActionProviders.Add (new ReviewPullRequestActionProvider (baseClient.WithRepository (repository), CancellationToken.None));
			//	ActionCentre.ActionProviders.Add (new VSMBuddyTestActionProvider (baseClient.WithRepository (repository), new Downloader ()));
			//}

			ActionCentre.Refresh ();
		}

		public override void WillTerminate(NSNotification notification)
		{
			// Insert code here to tear down your application
		}
	}
}
