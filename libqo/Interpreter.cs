using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace libqo
{
	public class Interpreter
	{
		/// <summary>
		/// A string that contains all valid qo identifiers.
		/// </summary>
		public const string SYMBOLS = "><+-*/.,:;[]()&\\^#@%$_\"";

		// Readonly private fields
		readonly SortedList<int, int> jumptable;
		readonly List<QoDebugMessage> errors;
		readonly bool hosted;

		// Private fields
		Stack<int> stack;
		int[] mem;
		bool jumptablebuilt;
		string source;
		int pos, memptr;
		bool wrap;

		/// <summary>
		/// Initializes a new instance of the <see cref="Interpreter"/> class.
		/// </summary>
		/// <param name="memsz">Memsz.</param>
		/// <param name="stacksz">Stacksz.</param>
		/// <param name="hosted">Should be true if called using <see cref="QoScriptHost"/>.</param> 
		public Interpreter (int memsz, int stacksz, bool hosted = false) {
			mem = new int[memsz];
			for (var i = 0; i < mem.Length; i++)
				mem [i] = 0;
			stack = new Stack<int> (stacksz);
			jumptable = new SortedList<int, int> ();
			errors = new List<QoDebugMessage> ();
			this.hosted = hosted;
		}

		/// <summary>
		/// Grab a new <see cref="Interpreter"/> instance. 
		/// </summary>
		/// <returns>A new instance.</returns>
		/// <param name="memsz">Memsz.</param>
		/// <param name="stacksz">Stacksz.</param>
		public static Interpreter GrabNew (int memsz, int stacksz) {
			return new Interpreter (memsz, stacksz);
		}

		public static Interpreter GrabHosted (int memsz, int stacksz) {
			return new Interpreter (memsz, stacksz, hosted: true);
		}

		/// <summary>
		/// Load qo sourcecode.
		/// </summary>
		/// <param name="source">Source.</param>
		public Interpreter Feed (string source) {
			jumptablebuilt = false;
			for (var i = 0; i < mem.Length; i++)
				mem [i] = 0;
			stack = new Stack<int> (stack.Count);
			this.source = source;
			return this;
		}

		/// <summary>
		/// Enable cell wrapping.
		/// </summary>
		/// <param name="wrap">If set to <c>true</c> wrap.</param>
		public Interpreter Wrap (bool wrap) {
			this.wrap = wrap;
			return this;
		}

		/// <summary>
		/// Get the value of a specific cell.
		/// </summary>
		/// <returns>The cell value.</returns>
		/// <param name="cell">Cell.</param>
		/// <param name="throwIfOutOfRange">If set to <c>true</c> throw if out of range.</param>
		public int GetCellValue (int cell, bool throwIfOutOfRange = true) {
			const string msg = "Cell must be greater than zero and less than the maximum cell length.";
			if (cell < 0 || cell > mem.Length)
			if (throwIfOutOfRange)
				throw new ArgumentOutOfRangeException (cell.ToString (), msg);
			else
				return 0;
			return mem [cell];
		}

		/// <summary>
		/// Return the top item on stack.
		/// </summary>
		/// <returns>The stack top.</returns>
		/// <param name="throwIfEmpty">If set to <c>true</c> throw if empty.</param>
		public int GetStackTop (bool throwIfEmpty = true) {
			if (stack.Count == 0)
			if (throwIfEmpty)
				throw new IndexOutOfRangeException ("Threre are no items on the stack.");
			else
				return 0;
			return stack.Peek ();
		}

		/// <summary>
		/// Resize the stack.
		/// You can only make the stack bigger, not smaller.
		/// </summary>
		/// <param name="newSize">New size.</param>
		public void ResizeStack (int newSize) {
			if (newSize <= stack.Count)
				throw new ArgumentException ("The new size is less or equal the current size.");
			var _stack = stack.ToList ();
			stack = new Stack<int> (newSize);
			for (var i = _stack.Count; i > 0;)
				stack.Push (--i);
		}

		/// <summary>
		/// Capture the current state of the interpreter.
		/// </summary>
		/// <returns>The state.</returns>
		public QoState GetState () {
			return new QoState {
				MemoryImage = (int [])mem.Clone (),
				StackImage = new Stack<int> (stack.Reverse ()),
				Errors = errors.ToList (),
			};
		}

		/// <summary>
		/// Interpret the loaded qo code.
		/// </summary>
		public void Interpret () {

			if (!BuildJumpTable ()) {
				Error ("Couldn't build jump table.");
				return;
			}

			// Interpret source
			while (InterpretOne ()) {
			}
		}

		/// <summary>
		/// Build the jump table.
		/// </summary>
		/// <returns>
		/// <c>true</c>, if jump table was built, <c>false</c> otherwise.
		/// </returns>
		public bool BuildJumpTable () {
			var loopstack = new Stack<int> ();
			for (var i = 0; i < source.Length; i++) {
				switch (source [i]) {
				case '[':
				case '(':
					loopstack.Push (i);
					break;
				case ']':
				case ')':
					if (loopstack.Count == 0) {
						Error ("Unmatched ']' at position {0}", i);
						return false;
					} else {
						var opening = loopstack.Pop ();
						jumptable.Add (opening, i);
						jumptable.Add (i, opening);
					}
					break;
				}
			}
			if (loopstack.Count > 0) {
				Error ("Unmatched '[' at position {0}", loopstack.Pop ());
				return false;
			}
			jumptablebuilt = true;
			return true;
		}

		/// <summary>
		/// Interpret a single instruction.
		/// </summary>
		/// <returns>
		/// <c>false</c> if the interpretation failed, <c>true</c> otherwise.
		/// </returns>
		public bool InterpretOne () {

			if (!jumptablebuilt) {
				Error ("You need to build the jump table!");
				Error ("Call BuildJumpTable before calling InterpretOne.");
				return false;
			}
			
			if (pos < source.Length) {

				switch (source [pos]) {
				case '>':
					memptr++;
					if (wrap && memptr >= mem.Length)
						memptr = 0;
					break;
				case '<':
					memptr--;
					if (wrap && memptr < 0)
						memptr = mem.Length - 1;
					break;
				case '+':
					mem [memptr]++;
					break;
				case '-':
					mem [memptr]--;
					break;
				case '*':
					mem [memptr] <<= 1;
					break;
				case '/':
					mem [memptr] >>= 1;
					break;
				case '.':
					Console.Write (Convert.ToChar (mem [memptr]));
					break;
				case ',':
					mem [memptr] = Console.Read ();
					break;
				case ':':
					stack.Push (mem [memptr]);
					break;
				case '&':
					stack.Push (stack.Peek ());
					break;
				case '^':
					memptr = stack.Pop ();
					break;
				case '#':
					mem [memptr] = stack.Count;
					break;
				case ';':
					mem [memptr] = stack.Pop ();
					break;
				case '\\':
					{
						int elem1 = stack.Pop ();
						int elem2 = stack.Pop ();
						stack.Push (elem1);
						stack.Push (elem2);
						break;
					}
				case '=':
					{
						int elem1 = stack.Pop ();
						int elem2 = stack.Pop ();
						mem [memptr] = elem1 == elem2 ? 1 : 0;
						break;
					}
				case '@':
					var collection = stack.ToList ();
					stack.Clear ();
					collection.ForEach (stack.Push);
					break;
				case '%':
					mem [memptr] = pos + 1;
					break;
				case '$':
					pos = mem [memptr];
					return true;
				case '_':
					mem [memptr] = source.Length;
					break;
				case '\'':
					while (source [pos] != '\n'
						&& pos < source.Length - 1)
						pos++;
					break;
				case '[':
					if (mem [memptr] == 0)
						pos = jumptable [pos];
					break;
				case ']':
					if (mem [memptr] != 0)
						pos = jumptable [pos];
					break;
				case '(':
					if (stack.Peek () == 0)
						pos = jumptable [pos];
					break;
				case ')':
					if (stack.Peek () != 0)
						pos = jumptable [pos];
					break;
				case '"':
					pos++;
					while (source [pos] != '"') {
						char chr = Convert.ToChar (source [pos++]);
						switch (chr) {
						case '\\':
							char next = Convert.ToChar (source [pos++]);
							switch (next) {
							case 'a':
								stack.Push ('\a');
								break;
							case 't':
								stack.Push ('\t');
								break;
							case 'n':
								stack.Push ('\n');
								break;
							case 'r':
								stack.Push ('\r');
								break;
							case '0':
								stack.Push (0);
								break;
							default:
								Error ("Invalid escape sequence: \\{0}", next);
								return false;
							}
							break;
						default:
							stack.Push (chr);
							break;
						}
					}
					break;
				default:
					if (char.IsWhiteSpace (source [pos]))
						break;
					Error ("Invalid instruction: {0}", source [pos]);
					return false;
				}

				pos++;
				return true;
			}
			return false;
		}

		public void Error (string format, params object[] args) {
			string msg = string.Format (format, args);
			if (!hosted)
				Console.WriteLine ("[ERROR] {0}", msg);
			errors.Add (new QoDebugMessage (GetState (), msg, pos, source [pos]));
		}

		public void Debug (string format, params object[] args) {
			#if DEBUG
			Console.WriteLine (format, args);
			#endif
		}
	}
}

