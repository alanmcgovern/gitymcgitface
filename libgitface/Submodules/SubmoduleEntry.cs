//
// Author: Adam Patridge
//

using System;
namespace libgitface
{
	/// <summary>
	/// Represents the minimum mandatory keys (per https://git-scm.com/docs/gitmodules.html).
	/// </summary>
	public class SubmoduleEntry
	{
		public string Branch { get; set; }
		public string Name { get; set; }
		public string Path { get; set; }
		public string Url { get; set; }
		// ?update
		// ?fetchRecurseSubmodules
		// ?ignore
		// ?shallow
	}
}
