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
using System.Threading.Tasks;
using Octokit;

namespace libgitface
{
	public class GitClient
	{
		public string BranchName { get; }
		public string BranchRef => "refs/heads/" + BranchName;
		Func<GitHubClient> ClientFactory { get; }
		public Repository Repository { get; }

		public GitClient (Func<GitHubClient> client)
			: this (client, null, null)
		{
			if (client == null)
				throw new ArgumentNullException (nameof (client));

			ClientFactory = client;
		}

		GitClient (Func<GitHubClient> client, Repository repository, string branchName)
		{
			BranchName = branchName ?? "master";
			ClientFactory = client;
			Repository = repository;
		}

		public async Task<bool> BranchExists (string branchName)
		{
			try {
				var branchRef = "refs/heads/" + branchName;
				await CreateClient ().Git.Reference.Get (Repository.Owner, Repository.Name, branchRef);
				return true;
			} catch (NotFoundException) {
				return false;
			}
		}

		public async Task<GitClient> CreateBranch (string branchName, string sha)
		{
			var branchRef = "refs/heads/" + branchName;
			await CreateClient ().Git.Reference.Create (Repository.Owner, Repository.Name, new NewReference (branchRef, sha));
			return WithBranch (branchName);
		}

		public async Task<string> CreateCommit (string message, string treeSha, string parentSha)
		{
			var newCommit = new NewCommit (message, treeSha, parentSha);
			var response = await CreateClient ().Git.Commit.Create (Repository.Owner, Repository.Name, newCommit);
			return response.Sha;
		}

		public async Task CreatePullRequest(string branchName, string titleText, string bodyText)
		{
			var newBranchRef = "refs/heads/" + branchName;
			var pr = new NewPullRequest (titleText, newBranchRef, BranchName) {
				Body = bodyText
			};
			await CreateClient ().PullRequest.Create (Repository.Owner, Repository.Name, pr);
		}

		public async Task<string> CreateTree (NewTree tree)
		{
			var response = await CreateClient ().Git.Tree.Create (Repository.Owner, Repository.Name, tree);
			return response.Sha;
		}

		public async Task<GitClient> DeleteBranch (string branchName)
		{
			var branchRef = "refs/heads/" + branchName;
			await CreateClient ().Git.Reference.Delete (Repository.Owner, Repository.Name, branchRef);
			return WithBranch (branchName);
		}

		public async Task<string> GetFileContent (string path)
		{
			var files = await CreateClient ().Repository.Content.GetAllContentsByRef (Repository.Owner, Repository.Name, path, BranchRef);
			if (files.Count != 1)
				throw new InvalidOperationException (string.Format ("Unexpected number of files returned from GitHub API. Expected '1' but got '{0}' for file {1}", files.Count, path)); 

			return files [0].Content;
		}

		public async Task<string> GetHeadSha ()
		{
			var result = await CreateClient ().Git.Tree.Get (Repository.Owner, Repository.Name, BranchRef);
			return result.Sha;
		}

		public async Task<IReadOnlyList<PullRequest>> GetPullRequests ()
		{
			return await CreateClient ().PullRequest.GetAllForRepository (Repository.Owner, Repository.Name);
		}

		public async Task<string> GetSubmoduleSha (SubmoduleEntry submodule)
		{
			var file = await CreateClient ().Repository.Content.GetAllContentsByRef (Repository.Owner, Repository.Name, submodule.Path, BranchRef);
			return file.Single ().Sha;
		}

		GitHubClient CreateClient ()
		{
			return ClientFactory ();
		}

		public GitClient WithBranch (string branchName)
		{
			return new GitClient (ClientFactory, Repository, branchName);
		}

		public GitClient WithRepository (Repository repository)
		{
			return new GitClient (ClientFactory, repository, BranchName);
		}
	}
}
