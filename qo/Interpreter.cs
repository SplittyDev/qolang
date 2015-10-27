using System;
using System.Collections.Generic;
using System.Linq;

namespace qo
{
	public class Interpreter
	{
		const string ALLOWED_ASCII_CHARS =
			"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890?!";
		
		public const string SYMBOLS = "><+-*/.,:;[]()&\\^#@%$_\"" + ALLOWED_ASCII_CHARS;

		readonly int[] mem;
		readonly Stack<int> stack;
		readonly SortedList<int, int> jumptable;
		bool jumptablebuilt;

		string source;
		int pos, memptr;
		bool wrap;

		public Interpreter (int memsz, int stacksz) {
			mem = new int[memsz];
			for (var i = 0; i < mem.Length; i++)
				mem [i] = 0;
			stack = new Stack<int> (stacksz);
			jumptable = new SortedList<int, int> ();
		}

		public static Interpreter GrabNew (int memsz, int stacksz) {
			return new Interpreter (memsz, stacksz);
		}

		public Interpreter Feed (string source) {
			this.source = source;
			return this;
		}

		public Interpreter Wrap (bool wrap) {
			this.wrap = wrap;
			return this;
		}

		public void Interpret () {

			if (!BuildJumpTable ()) {
				Console.WriteLine ("[ERROR] Couldn't build jump table.");
				return;
			}

			// Interpret source
			while (InterpretOne ()) {
			}
		}

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
						Console.Error.WriteLine ("[ERROR] Unmatched ']' at position {0}", i);
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
				Console.Error.WriteLine ("[ERROR] Unmatched '[' at position {0}", loopstack.Pop ());
				return false;
			}
			jumptablebuilt = true;
			return true;
		}

		public bool InterpretOne () {

			if (!jumptablebuilt) {
				Console.Error.WriteLine ("[ERROR] You need to build the jump table!");
				Console.Error.WriteLine ("[ERROR] Call BuildJumpTable before calling Interpret.");
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
				case '"':
					var _stack = stack.Reverse ().ToArray ();
					var start = _stack.First (x => x == 0);
					var stop = stack.Count - start - 1;
					var spos = start;
					while (spos < stop) {
						stack.Pop ();
						Console.Write (Convert.ToChar (_stack.ElementAt (spos++)));
					}
					stack.Pop ();
					break;
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
				default:
					if ((int)source [pos] <= 0xFF
					    && ALLOWED_ASCII_CHARS.Contains (source [pos].ToString ())) {
						stack.Push ((int)source [pos]);
					}
					pos++;
					return true;
				}

				pos++;
				return true;
			}
			return false;
		}
	}
}

