using System;
using System.Collections.Generic;

namespace libgitface
{
	public static class BranchMapper
	{
		static readonly Dictionary<string, string> VSMBranchMapper = new Dictionary<string, string> {
			{ "master", "master" },
			{ "d15-8",  "release-7.6" },
		};

		static readonly Dictionary<string, string> MonoBranchMapper = new Dictionary<string, string> {
			{ "master", "2018-06" },
			{ "d15-8",  "2018-02" },
		};

		/// <summary>
		/// Converts a release branch name (d15-8) into a mono branch name (2018-02)
		/// </summary>
		/// <returns>The VSMB ranch.</returns>
		/// <param name="branch">Branch.</param>
		public static string ToMonoBranch (string branch)
		{
			return MonoBranchMapper [branch];
		}

		/// <summary>
		/// Converts a release branch name (d15-8) into a VSM branch name (release-7.6)
		/// </summary>
		/// <returns>The VSMB ranch.</returns>
		/// <param name="branch">Branch.</param>
		public static string ToVSMBranch (string branch)
		{
			return VSMBranchMapper [branch];
		}
	}
}
