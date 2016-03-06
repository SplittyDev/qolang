using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;

namespace libqo {
	
	public class QoState {

		public string Output;
		public string Input;
		public int CellPointer;
		public int InputPointer;
		public int ProgramCounter;
		public int[] MemoryImage;
		public int[] StackImage;
		public Interpreter Interpreter;
		public QoDebugMessage[] Errors;
		public QoFunction[] Functions;

		internal QoState () {
			Interpreter = null;
			Input = string.Empty;
			Output = string.Empty;
			MemoryImage = new int[0];
			StackImage = new int[0];
			Functions = new QoFunction[0];
			Errors = new QoDebugMessage[0];
		}

		public QoState (Interpreter interpreter) {
			Interpreter = interpreter;
			Input = interpreter.Input;
			Output = interpreter.Output;
			ProgramCounter = interpreter.ProgramCounter;
			CellPointer = interpreter.CellPointer;
			InputPointer = interpreter.InputPointer;
			MemoryImage = interpreter.Memory;
			StackImage = interpreter.Stack;
			Functions = interpreter.Functions;
			Errors = interpreter.Errors;
		}

		public int[] GetStackTape () {
			return StackImage;
		}

		public int[] GetReversedStackTape () {
			return GetStackTape ()
				.Reverse ()
				.ToArray ();
		}
	}
}

