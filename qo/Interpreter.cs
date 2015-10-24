using System;
using System.Collections.Generic;

namespace qo
{
	public class Interpreter
	{
		readonly int[] mem;
		readonly Stack<int> stack;

		string source;
		int pos = -1, memptr;

		public Interpreter (int memsz, int stacksz) {
			mem = new int[memsz];
			stack = new Stack<int> (stacksz);
		}

		public static Interpreter GrabNew (int memsz, int stacksz) {
			return new Interpreter (memsz, stacksz);
		}

		public Interpreter Feed (string source) {
			this.source = source;
			return this;
		}

		public void Interpret () {
			
		}
	}
}

