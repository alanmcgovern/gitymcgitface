using System;
using System.Collections.Generic;
using System.Linq;

namespace libgitface
{
	public static class Groupings
	{
		public static readonly IEqualityComparer<string[]> GroupingComparer = new Comparer ();
		public const string Bump = nameof (Bump);
		public const string Direct = nameof (Direct);
		public const string PR = nameof (PR);
		public static readonly string[] BumpPullRequest = new string[] { Bump, PR };

		public const string BuddyTest = nameof (BuddyTest);
		public const string Review = nameof (Review);
		public const string Merge = nameof (Merge);


		class Comparer : IEqualityComparer<string[]>
		{
			public bool Equals(string[] x, string[] y)
			{
				if (x == null)
					return y == null;
				if (y == null)
					return false;
				return x.FirstOrDefault () == y.FirstOrDefault ();
			}

			public int GetHashCode(string[] obj)
			{
				return obj?.FirstOrDefault ()?.GetHashCode () ?? 0;
			}
		}

	}
}
