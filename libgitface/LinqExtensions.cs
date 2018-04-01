using System;
using System.Collections.Generic;
using System.Linq;

namespace libgitface
{
	public static class LinqExtensions
	{
		class DelegateComparer<T, TCompare> : IEqualityComparer<T>
		{
			Func<T, TCompare> Selector;

			public DelegateComparer (Func<T, TCompare> selector) => Selector = selector;

			public bool Equals (T x, T y) => EqualityComparer<TCompare>.Default.Equals (Selector (x), Selector (y));

			public int GetHashCode (T obj) => EqualityComparer<TCompare>.Default.GetHashCode (Selector (obj));
		}

		public static IEnumerable<T> Distinct<T, TCompare> (this IEnumerable<T> self, Func<T, TCompare> selector)
		{
			var comparer = new DelegateComparer<T, TCompare> (selector);
			return self.Distinct (comparer);
		}
	}
}
