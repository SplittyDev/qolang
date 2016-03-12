using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace libqo
{
	public class Interpreter {

		#region Constants

		/// <summary>
		/// A string that contains all valid qo identifiers.
		/// </summary>
		public const string SYMBOLS = "=><+-*/.,:;[](){}&\\^#@$\"cbixq";
		#endregion

		#region Type definitions

		/// <summary>
		/// Number input/output mode.
		/// </summary>
		enum NumberMode { Char, Bin, Dec, Hex };
		#endregion

		#region Fields

		// Readonly private fields
		readonly SortedList<int, int> jumptable;
		readonly bool hosted;
		readonly bool shadowed;

		// Private fields
		StringBuilder output;
		NumberMode numbermode;
		Stack<int> stack;
		List<QoFunction> functions;
		List<QoDebugMessage> errors;
		int[] mem;
		bool jumptablebuilt;
		string source;
		int pos, memptr;
		bool wrap;
		string input;
		int inputpos;
		#endregion

		#region Properties

		/// <summary>
		/// Gets the memory image.
		/// </summary>
		/// <value>The memory.</value>
		public int[] Memory {
			get { return (int[]) mem.Clone (); }
		}

		/// <summary>
		/// Gets the stack image.
		/// </summary>
		/// <value>The stack.</value>
		public int[] Stack {
			get { return stack.ToArray (); }
		}

		/// <summary>
		/// Gets the input image.
		/// </summary>
		/// <value>The input.</value>
		public string Input {
			get { return input; }
		}

		/// <summary>
		/// Gets the output image.
		/// </summary>
		/// <value>The output.</value>
		public string Output {
			get { return output.ToString (); }
		}

		/// <summary>
		/// Gets the program counter image.
		/// </summary>
		/// <value>The program counter.</value>
		public int ProgramCounter {
			get { return pos; }
		}

		/// <summary>
		/// Gets the cell pointer image.
		/// </summary>
		/// <value>The cell pointer.</value>
		public int CellPointer {
			get { return memptr; }
		}

		/// <summary>
		/// Gets the input pointer image.
		/// </summary>
		/// <value>The input pointer.</value>
		public int InputPointer {
			get { return inputpos; }
		}

		/// <summary>
		/// Gets the function list image.
		/// </summary>
		/// <value>The functions.</value>
		public QoFunction[] Functions {
			get { return functions.ToArray (); }
		}

		/// <summary>
		/// Gets the error list image.
		/// </summary>
		/// <value>The errors.</value>
		public QoDebugMessage[] Errors {
			get { return errors.ToArray (); }
		}
		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="libqo.Interpreter"/> class.
		/// </summary>
		Interpreter (bool hosted = false, bool shadowed = false) {
			output = new StringBuilder ();
			jumptable = new SortedList<int, int> ();
			errors = new List<QoDebugMessage> ();
			functions = new List<QoFunction> ();
			this.hosted = hosted;
			this.shadowed = shadowed;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Interpreter"/> class.
		/// </summary>
		/// <param name="memsz">Memsz.</param>
		/// <param name="stacksz">Stacksz.</param>
		/// <param name="hostedmode">Should be true if called using <see cref="QoScriptHost"/>.</param> 
		public Interpreter (int memsz, int stacksz, bool hostedmode = false)
			: this (hosted: hostedmode, shadowed: false) {
			mem = new int[memsz].ZeroFill ();
			stack = new Stack<int> (stacksz);
		}
		#endregion

		#region Public members

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
			return new Interpreter (memsz, stacksz, true);
		}

		public static Interpreter GrabShadow (Interpreter shadowee) {
			var interpreter = new Interpreter (shadowee.hosted, true);
			interpreter.SetState (shadowee.GetState ());
			return interpreter;
		}

		/// <summary>
		/// Load qo sourcecode.
		/// </summary>
		/// <param name="source">Source.</param>
		public Interpreter Feed (string source) {
			jumptablebuilt = false;
			this.source = source;
			if (shadowed)
				return this;
			for (var i = 0; i < mem.Length; i++)
				mem [i] = 0;
			stack = new Stack<int> (stack.Count);
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
		    if (cell >= 0 && cell <= mem.Length)
		        return mem[cell];
		    if (throwIfOutOfRange)
		        throw new ArgumentOutOfRangeException (cell.ToString (), msg);
		    return 0;
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
				throw new ArgumentException ("The new size is less than or equal the current size.");
			var _stack = stack.ToList ();
			stack = new Stack<int> (newSize);
			for (var i = _stack.Count; i > 0;)
				stack.Push (_stack [--i]);
		}

		/// <summary>
		/// Capture the current state of the interpreter.
		/// </summary>
		/// <returns>The state.</returns>
		public QoState GetState () {
			return new QoState (this);
		}

		/// <summary>
		/// Feeds input characters to the interpreter.
		/// Will be used in hosted mode instead of stdin.
		/// </summary>
		/// <param name="str">String.</param>
		public Interpreter FeedInput (string str) {
			input = str;
			return this;
		}

		/// <summary>
		/// Set the current state of the interpreter.
		/// </summary>
		/// <param name="state">State.</param>
		public void SetState (QoState state) {
			mem = state.MemoryImage;
			memptr = state.CellPointer;
			stack = new Stack<int> (state.GetReversedStackTape ());
			output = new StringBuilder (state.Output);
			errors = new List<QoDebugMessage> (state.Errors);
			functions = new List<QoFunction> (state.Functions);
			input = state.Input;
			inputpos = state.InputPointer;
		}

		/// <summary>
		/// Interpret the loaded qo code.
		/// </summary>
		public Interpreter Interpret () {

			// Build the jump table
			if (!BuildJumpTable ()) {
				Error ("Could not build the jump table.");
				return this;
			}

			// Interpret source
			while (true) {
				if (InterpretOne ())
					continue;
				break;
			}

			return this;
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
				Error ("Call BuildJumpTable before calling Interpret or InterpretOne.");
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
					var val = mem [memptr];
					switch (numbermode) {
					case NumberMode.Char:
						WriteOutput (Convert.ToChar (val).ToString ());
						break;
					case NumberMode.Bin:
						WriteOutput (Convert.ToString (val, 2));
						break;
					case NumberMode.Dec:
						WriteOutput (Convert.ToString (val, 10));
						break;
					case NumberMode.Hex:
						WriteOutput (Convert.ToString (val, 16));
						break;
					}
					break;
				case ',':
					char inchr = '\x00';
					if (hosted) {
						if (!string.IsNullOrEmpty (input)
						    && inputpos < input.Length) {
							inchr = input [inputpos++];
							WriteOutput (inchr.ToString ());
						} else {
							Error ("No input available");
							return false;
						}
					}
					else
						inchr = Console.ReadKey ().KeyChar;
					if (numbermode != NumberMode.Char && !char.IsDigit (inchr)) {
						Error ("Unexpected char in input sequence: {0}", inchr);
						return false;
					}
					switch (numbermode) {
					case NumberMode.Char:
						mem [memptr] = inchr;
						break;
					default:
						mem [memptr] = int.Parse (inchr.ToString ());
						break;
					}
					break;
				case ':':
					stack.Push (mem [memptr]);
					break;
				case '&':
					stack.Push (stack.Peek ());
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
				case '$':
					pos++;
					EatWhitespace ();
					var funcname = new StringBuilder ();
					if (pos >= source.Length) {
						Error ("Unexpected end of file");
						return false;
					}
					while (pos < source.Length
					       && (char.IsLetter (source [pos])
					       || "_".Contains (source [pos]))) {
						funcname.Append (source [pos++]);
					}
					EatWhitespace ();
					if (pos < source.Length && source [pos] == '{') {
						pos++;
						var funcsrc = new StringBuilder ();
						while (source [pos] != '}')
							funcsrc.Append (source [pos++]);
						var function = new QoFunction (funcname, funcsrc);
						if (functions.All (func => function.Name != func.Name))
							functions.Add (function);
						else {
							Error ("Function already defined: {0}", funcname);
							return false;
						}
					} else {
						var function = functions
							.FirstOrDefault (func =>
								func.Name == funcname.ToString ());
						if (function == default (QoFunction)) {
							Error ("Call to undefined function: {0}", funcname);
							return false;
						}
						function.Call (this);
						return true;
					}
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
				    var chars = new List<int> ();
					pos++;
					while (source [pos] != '"') {
						var chr = Convert.ToChar (source [pos++]);
						switch (chr) {
						case '\\':
							var next = Convert.ToChar (source [pos++]);
							switch (next) {
							case 'a':
								chars.Add ('\a');
								break;
							case 't':
								chars.Add ('\t');
								break;
							case 'n':
								chars.Add ('\n');
								break;
							case 'r':
								chars.Add ('\r');
								break;
							case '0':
								chars.Add (0);
								break;
							default:
								Error ("Invalid escape sequence: \\{0}", next);
								return false;
							}
							break;
						default:
							chars.Add (chr);
							break;
						}
					}
				    foreach (var chr in chars.Reverse<int> ())
				        stack.Push (chr);
					break;
				case 'c':
					numbermode = NumberMode.Char;
					break;
				case 'b':
					numbermode = NumberMode.Bin;
					break;
				case 'i':
					numbermode = NumberMode.Dec;
					break;
				case 'x':
					numbermode = NumberMode.Hex;
					break;
				case 'q':
					pos = source.Length;
					return false;
				default:
					if (char.IsWhiteSpace (source [pos]))
						break;
					Error ("Invalid instruction at {0}: {1}", pos, source [pos]);
					return false;
				}

				pos++;
				return true;
			}
			return false;
		}

		#endregion

		#region Private members

		/// <summary>
		/// Emits an error message.
		/// </summary>
		/// <param name="format">Format.</param>
		/// <param name="args">Arguments.</param>
		void Error (string format, params object[] args) {
			string msg = string.Format (format, args);
			if (!hosted)
				Console.WriteLine ("[ERROR] {0}", msg);
			var debugmsg = new QoDebugMessage (
				state: GetState (),
				msg: msg,
				pos: pos,
				chr: pos < source.Length ? source [pos] : '\0'
			);
			errors.Add (debugmsg);
		}

		/// <summary>
		/// Writes something to the output buffer
		/// Also writes to the console if in standalone mode
		/// </summary>
		/// <param name="str">String.</param>
		void WriteOutput (string str) {
			if (!hosted)
				Console.Write (str);
			output.Append (str);
		}

		/// <summary>
		/// Emits a debug message.
		/// </summary>
		/// <param name="format">Format.</param>
		/// <param name="args">Arguments.</param>
		internal static void Debug (string format, params object[] args) {
			#if DEBUG
			Console.WriteLine (format, args);
			#endif
		}
		#endregion

		#region Lexer helper functions
		void EatWhitespace () {
			while (pos < source.Length && char.IsWhiteSpace (source [pos]))
				pos++;
		}
		#endregion
	}
}

