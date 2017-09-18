using System;
using System.Linq;
using libgitface;
using NUnit.Framework;

namespace libgitfacetests
{
	[TestFixture]
	public class GitModulesParserTests
	{
		[Test]
		public void DefaultBranchIsMaster ()
		{
			var parser = new GitModulesParser ();
			var result = parser.Parse (@"
[submodule ""MyName""]
	path = full/path/to/submodule
	url = ../../repo/submodule").Single ();

			Assert.AreEqual ("master", result.Branch);
		}

		[Test]
		public void RemoveTrailingDotGit ()
		{
			var parser = new GitModulesParser ();
			var result = parser.Parse (@"
[submodule ""MyName""]
	url = ../../repo/submodule.git").Single ();

			Assert.AreEqual ("../../repo/submodule", result.Url);
		}

		[Test]
		public void SingleModule ()
		{
			var parser = new GitModulesParser ();
			var result = parser.Parse (@"
[submodule ""MyName""]
	path = full/path/to/submodule
	url = ../../repo/submodule
	branch = feature_branch").Single ();

			Assert.AreEqual ("MyName", result.Name);
			Assert.AreEqual ("full/path/to/submodule", result.Path);
			Assert.AreEqual ("../../repo/submodule", result.Url);
			Assert.AreEqual ("feature_branch", result.Branch);
		}

		[Test]
		public void TwoModules ()
		{
			var parser = new GitModulesParser ();
			var result = parser.Parse (@"
[submodule ""One""]
	path = path/one
	url = url/one
[submodule ""Two""]
	path = path/two
	url = url/two").ToArray ();

			Assert.AreEqual ("One", result [0].Name);
			Assert.AreEqual ("path/one", result [0].Path);
			Assert.AreEqual ("url/one", result [0].Url);

			Assert.AreEqual ("Two", result [1].Name);
			Assert.AreEqual ("path/two", result [1].Path);
			Assert.AreEqual ("url/two", result [1].Url);
		}
	}
}
