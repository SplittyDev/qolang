using System;

namespace libqo
{
	public class QoDebugMessage
	{
		public readonly QoState State;
		public readonly string Message;
		public readonly int Position;
		public readonly char Char;

		public QoDebugMessage (QoState state, string msg, int pos, char chr) {
			State = state;
			Message = msg;
			Position = pos;
			Char = chr;
		}
	}
}

