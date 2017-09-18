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
using System.Linq;
using System.Threading.Tasks;
using Octokit;

namespace libgitface.ActionProviders
{
	public abstract class GitHubActionProvider : ActionProvider
	{
		Func<GitHubClient> ClientFactory {
			get;
		}

		protected Repository Repository {
			get;
		}

		public GitHubActionProvider (Func<GitHubClient> client, Repository repository)
		{
			if (client == null)
				throw new ArgumentNullException (nameof (client));

			ClientFactory = client;
			Repository = repository;
		}

		protected GitHubClient CreateClient ()
		{
			return ClientFactory ();
		}

		protected async Task<string> GetFileContent (string path, string reference)
		{
			var files = await CreateClient ().Repository.Content.GetAllContentsByRef (Repository.Owner, Repository.Name, path, reference);
			if (files.Count != 1)
				throw new InvalidOperationException (string.Format ("Unexpected number of files returned from GitHub API. Expected '1' but got '{0}' for file {1}", files.Count, path)); 

			return files [0].Content;
		}
	}
}
