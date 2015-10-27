using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace qo
{
	public static class StringExtensions
	{
		public static string ToQo (this string str) {
			var accum = new StringBuilder ();
			for (var i = 0; i < str.Length; i++) {
				if (str [i] >= 'A' && str [i] <= 'Z' ||
				    str [i] >= 'a' && str [i] <= 'z' ||
				    str [i] >= '0' && str [i] <= '9')
					accum.Append (str [i]);
				else
					accum.AppendFormat ("{0}:[-]", string.Empty.PadLeft (str [i], '+'));
			}
			accum.Append ("[-]:\"");
			return accum.ToString ();
		}
	}
}

