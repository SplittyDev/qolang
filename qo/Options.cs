using System;
using Codeaddicts.libArgument.Attributes;

namespace qo
{
	public class Options
	{
		public const int DEFAULT_MEMORY_SIZE = 30000;
		public const int DEFAULT_STACK_SIZE = 512;

		[Docs ("Input file")]
		#if WINDOWS
		[Argument ("/i")]
		#else
		[Argument ("-i")]
		#endif
		public string input;

		[Docs ("Memory size in bytes")]
		#if WINDOWS
		[Argument ("/mem")]
		#else
		[Argument ("--mem")]
		#endif
		public string memsz;

		[Docs ("Stack size in bytes")]
		#if WINDOWS
		[Argument ("/stack")]
		#else
		[Argument ("--stack")]
		#endif
		public string stacksz;

		[Docs ("Show this help")]
		#if WINDOWS
		[Switch ("/?")]
		#else
		[Switch ("-h", "--help")]
		#endif
		public bool help;

		[Docs ("Show version")]
		#if WINDOWS
		[Switch ("/v", "/version")]
		#else
		[Switch ("-v", "--version")]
		#endif
		public bool version;

		[Docs ("Wrap cells (default off)")]
		#if WINDOWS
		[Switch ("/wrap")]
		#else
		[Switch ("--wrap")]
		#endif
		public bool wrap;

		[Docs ("Minify code")]
		#if WINDOWS
		[Switch ("/minify")]
		#else
		[Switch ("--minify")]
		#endif
		public bool minify;

		[Docs ("Convert a string to qo code")]
		#if WINDOWS
		[Switch ("/qoify")]
		#else
		[Switch ("--qoify")]
		#endif
		public bool qoify;

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

