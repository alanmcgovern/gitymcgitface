using System;
namespace libgitface
{
	public static class Log
	{
		public static void Exception (string message, Exception ex)
		{
			Raw ("{0} - {1}", message, ex);
		}

		public static void Raw (string format, params object[] args)
		{
			Console.WriteLine (format, args);
		}
	}
}
