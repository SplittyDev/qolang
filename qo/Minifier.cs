﻿using System;
using System.Text;

namespace qo
{
	public class Minifier
	{
		string source;
		int pos;

		public Minifier () {
		}

		public static Minifier GrabNew () {
			return new Minifier ();
		}

		public Minifier Feed (string source) {
			this.source = source;
			return this;
		}

		public string Minify () {
			var accum = new StringBuilder ();
			while (pos < source.Length) {
				if (source [pos] == '\'') {
					while (pos < source.Length && source [pos] != '\n')
						pos++;
					continue;
				}
				if (Interpreter.SYMBOLS.Contains (source [pos].ToString ()))
					accum.Append (source [pos]);
				pos++;
			}
			return accum.ToString ();
		}
	}
}

