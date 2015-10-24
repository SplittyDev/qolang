using System;
using Codeaddicts.libArgument.Attributes;

namespace qo
{
	public class Options
	{
		public const int DEFAULT_MEMORY_SIZE = 512;
		public const int DEFAULT_STACK_SIZE = 64;

		[Docs ("Input file")]
		[Argument ("-i", "/i")]
		public string input;

		[Docs ("Read input from stdin")]
		[Switch ("--stdin", "/stdin")]
		public bool input_stdin;

		[Docs ("Memory size in bytes")]
		[Argument ("-m", "--mem", "/mem")]
		public string memsz;

		[Docs ("Stack size in bytes")]
		[Argument ("-s", "--stack", "/stack")]
		public string stacksz;

		/// <summary>
		/// Gets the size of the memory.
		/// </summary>
		/// <value>The size of the memory.</value>
		public int MemorySize {
			get {
				int sz;
				return string.IsNullOrEmpty (memsz) ? DEFAULT_MEMORY_SIZE :
					!int.TryParse (memsz, out sz) ? DEFAULT_MEMORY_SIZE : sz;
			}
		}

		/// <summary>
		/// Gets the size of the stack.
		/// </summary>
		/// <value>The size of the stack.</value>
		public int StackSize {
			get {
				int sz;
				return string.IsNullOrEmpty (stacksz) ? DEFAULT_STACK_SIZE :
					!int.TryParse (stacksz, out sz) ? DEFAULT_STACK_SIZE : sz;
			}
		}
	}
}

