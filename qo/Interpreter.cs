﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace qo
{
	public class Interpreter
	{
		readonly string ALLOWED_ASCII_CHARS =
			"abcdefghijklmnopqrstuvwxyzABCEFGHIJKLMNOPQRSTUVWXYZ1234567890?!";

		readonly int[] mem;
		readonly Stack<int> stack;

		string source;
		int pos, memptr;

		public Interpreter (int memsz, int stacksz) {
			mem = new int[memsz];
			for (var i = 0; i < mem.Length; i++)
				mem [i] = 0;
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
			
			// Iterate over the source string
			while (pos < source.Length) {

				switch (source [pos]) {
				case '>':
					memptr++;
					break;
				case '<':
					memptr--;
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
				case '^':
					mem [memptr] *= stack.Pop ();
					break;
				case '#':
					mem [memptr] = stack.Count;
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
				case ';':
					mem [memptr] = stack.Pop ();
					break;
				case '&':
					stack.Push (stack.Peek ());
					break;
				case '|':
					stack.Pop ();
					break;
				case '\\':
					int elem1 = stack.Pop ();
					int elem2 = stack.Pop ();
					stack.Push (elem1);
					stack.Push (elem2);
					break;
				case '[':
					if (mem [memptr] == 0)
						while (source [pos] != ']')
							pos++;
					break;
				case ']':
					if (mem [memptr] != 0)
						while (source [pos] != '[')
							pos--;
					break;
				case '(':
					if (stack.Peek () == 0)
						while (source [pos] != ')')
							pos++;
					break;
				case ')':
					if (stack.Peek () != 0)
						while (source [pos] != '(')
							pos--;
					break;
				case '\'':
					while (source [pos] != '\n'
						&& pos < source.Length - 1)
						pos++;
					break;
				case '@':
					var collection = stack.ToList ();
					stack.Clear ();
					collection.ForEach (stack.Push);
					break;
				case '_':
					mem [memptr] = pos;
					break;
				case '$':
					pos = mem [memptr];
					break;
				default:
					if ((int)source [pos] <= 0xFF
						&& ALLOWED_ASCII_CHARS.Contains (source [pos].ToString ())) {
						stack.Push ((int)source [pos]);
					}
					pos++;
					continue;
				}

				pos++;
			}
		}
	}
}

