using System;
using System.Text;

namespace libqo {
	public class QoFunction {

		public string Name;
		public string Source;

		public QoFunction (string name, string source) {
			Name = name;
			Source = source;
		}

		public QoFunction (StringBuilder name, StringBuilder source)
			: this (name.ToString (), source.ToString ()) {
		}

		public void Call (Interpreter shadowee) {
			var oldstate = shadowee.GetState ();
			var newstate = Interpreter
				.GrabShadow (shadowee)
				.Feed (Source)
				.Interpret ()
				.GetState ();
			shadowee.SetState (newstate);
		}
	}
}

