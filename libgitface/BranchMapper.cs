using System;
using System.Collections.Generic;

namespace libgitface
{
	public static class BranchMapper
	{
		static readonly Dictionary<string, string> VSMBranchMapper = new Dictionary<string, string> {
			{ "master", "master" },
			{ "d16-3",  "release-8.3" },
			{ "d16-2",  "release-8.2" },
			{ "d16-1-new-document-model",  "release-8.1" },
			{ "d16-1",  "release-8.1" },
			{ "d16-0",  "release-8.0" },
			{ "d15-9",  "release-7.7" },
		};

		static readonly Dictionary<string, string> MonoBranchMapper = new Dictionary<string, string> {
			{ "master", "2019-02" },
			{ "d16-3",  "2019-02" },
			{ "d16-2",  "2019-02" },
			{ "d16-1",  "2018-08" },
			{ "d16-0",  "2018-08" },
			{ "d15-9",  "2018-06" },
		};

		static readonly Dictionary<string, string> XamarinAndroidBranchMapper = new Dictionary<string, string> {
			{ "master", "master" },
			{ "d16-3",  "d16-3" },
			{ "d16-2",  "d16-2" },
			{ "d16-1",  "d16-1" },
			{ "d16-0",  "d16-0" },
			{ "d15-9",  "d15-9" },
		};
		static readonly Dictionary<string, string> XamariniOSBranchMapper = new Dictionary<string, string> {
			{ "master", "master" },
			{ "d16-3",  "d16-3" },
			{ "d16-2",  "d16-2" },
			{ "d16-1",  "d16-1" },
			{ "d16-0",  "d16-0" },
			{ "d15-9",  "d15-9" },
		};
		static readonly Dictionary<string, string> VisualStudioBranchMapper = new Dictionary<string, string> {
			{ "master", "master" },
			{ "d16-3",  "d16-3" },
			{ "d16-2",  "d16-2" },
			{ "d16-1",  "d16-1" },
			{ "d16-1-new-document-model",  "d16-1" },
			{ "d16-0",  "d16-0" },
			{ "d15-9",  "d15-9" },
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
