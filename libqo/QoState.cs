using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;

namespace libqo
{
	[StructLayout (LayoutKind.Sequential)]
	public struct QoState
	{
		public int[] MemoryImage;
		public Stack<int> StackImage;
		public List<QoDebugMessage> Errors;

		public int[] GetStackTape () {
			return StackImage
				.ToArray ();
		}

		public int[] GetReversedStackTape () {
			return GetStackTape ()
				.Reverse ()
				.ToArray ();
		}
	}
}

