using System;
using System.Collections.Generic;

namespace libgitface
{
	public static class BranchMapper
	{
		static readonly Dictionary<string, string> VSMBranchMapper = new Dictionary<string, string> {
			{ "master", "master" },
			{ "d16-0-p1", "release-8.0" },
			{ "d15-9",  "release-7.7" },
			{ "d15-8-xcode10-1", "release-7.6-xcode10.1" },
			{ "d15-8",  "release-7.6" },
		};

		static readonly Dictionary<string, string> MonoBranchMapper = new Dictionary<string, string> {
			{ "master", "2018-08" },
			{ "d16-0-p1",  "2018-08" },
			{ "d15-9",  "2018-06" },
			{ "d15-8-xcode10-1",  "2018-02" },
			{ "d15-8",  "2018-02" },
		};

		static readonly Dictionary<string, string> XamarinAndroidBranchMapper = new Dictionary<string, string> {
			{ "master", "master" },
			{ "d16-0-p1", "d16-0-p1" },
			{ "d15-9", "d15-9" },
			{ "d15-8-xcode10-1",  "d15-8" },
			{ "d15-8", "d15-8" },
		};
		static readonly Dictionary<string, string> XamariniOSBranchMapper = new Dictionary<string, string> {
			{ "master", "master" },
			{ "d16-0-p1", "d16-0-p1" },
			{ "d15-9", "d15-9" },
			{ "d15-8-xcode10-1",  "d15-8" },
			{ "d15-8", "xcode10" },
		};
		static readonly Dictionary<string, string> VisualStudioBranchMapper = new Dictionary<string, string> {
			{ "master", "master" },
			{ "d16-0-p1", "d16-0-p1" },
			{ "d15-9", "d15-9" },
			{ "d15-8-xcode10-1",  "d15-8-xcode101" },
			{ "d15-8", "d15-8" },
		};

		/// <summary>
		/// Converts the designer's release branch name (d15-8) into a mono branch name (2018-02)
		/// </summary>
		/// <returns>The VSMB ranch.</returns>
		/// <param name="branch">Branch.</param>
		public static string ToMonoBranch (string branch)
		{
			return MonoBranchMapper [branch];
		}

		public static string ToVisualStudioBranch (string branch)
		{
			return VisualStudioBranchMapper [branch];
		}

		/// <summary>
		/// Converts the designer's release branch name (d15-8) into a VSM branch name (release-7.6)
		/// </summary>
		/// <returns>The VSMB ranch.</returns>
		/// <param name="branch">Branch.</param>
		public static string ToVSMBranch (string branch)
		{
			return VSMBranchMapper [branch];
		}

		/// <summary>
		/// Converts the designer's release branch name (d15-7-xcode10) into a Xamarin.Android branch name (xcode10)
		/// </summary>
		/// <returns>The VSMB ranch.</returns>
		/// <param name="branch">Branch.</param>
		public static string ToXamarinAndroidBranch (string branch)
		{
			return XamarinAndroidBranchMapper [branch];
		}

		/// <summary>
		/// Converts the designer's release branch name (d15-7-xcode10) into a Xamarin.iOS branch name (xcode10)
		/// </summary>
		/// <returns>The VSMB ranch.</returns>
		/// <param name="branch">Branch.</param>
		public static string ToXamariniOSBranch (string branch)
		{
			return XamariniOSBranchMapper [branch];
		}
	}
}
