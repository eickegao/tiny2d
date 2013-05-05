using System;
using System.Diagnostics;

namespace Microsoft.Xna.Framework
{
	public static class ExEnLog
	{
		[Conditional("DEBUG")]
		public static void WriteLine(string message)
		{
			Android.Util.Log.WriteLine(Android.Util.LogPriority.Info,
					"ExEn", message);
		}
	}
}

