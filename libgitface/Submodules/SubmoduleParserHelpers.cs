using System;

namespace libgitface
{
	static class SubmoduleParserHelpers
	{
		public static string TakeBetween (this string line, char startChar, char endChar)
		{
			var start = line.IndexOf (startChar);
			if (start == -1)
				throw new ArgumentException (string.Format ("Could not parse quoted string start from '{0}'", line));

			// Skip the quote
			start += 1;
			var end = line.IndexOf (endChar, start);
			if (end == -1)
				throw new ArgumentException (string.Format ("Could not parse quoted string end from '{0}'", line));

			return line.Substring (start, end - start).Trim (); 
		}

		public static string TakeAfter (this string line, char separator)
		{
			var start = line.IndexOf (separator);
			if (start == -1)
				throw new ArgumentException (string.Format ("Could not find separator '{0}' in string '{1}'", separator, line));

			start += 1;
			return line.Substring (start).Trim ();
		}
	}
}
