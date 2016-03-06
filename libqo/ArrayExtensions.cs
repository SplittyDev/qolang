using System;

namespace libqo {

	/// <summary>
	/// Array extensions.
	/// </summary>
	public static class ArrayExtensions {

		/// <summary>
		/// Fills an int array with zero's.
		/// </summary>
		/// <returns>The zero-filled array.</returns>
		/// <param name="array">Array.</param>
		public static int[] ZeroFill (this int[] array) {
			for (var i = 0; i < array.Length; i++)
				array [i] = 0;
			return array;
		}
	}
}

