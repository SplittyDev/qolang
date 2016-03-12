using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace libqo {
	
	public class QoScriptHost {
		
		const int RECOMMENDED_TAPE_SIZE = 30000;
		const int RECOMMENDED_STACK_SIZE = 512;

		readonly Minifier Minifier;
		string Input;

		QoScriptHost () {
			Input = string.Empty;
			Minifier = Minifier.GrabNew ();
		}

		public static QoScriptHost GrabNew () {
			return new QoScriptHost ();
		}

		public QoScriptHost SetInput (string input) {
			Input = input;
			return this;
		}

		public QoState Interpret (string code,
			int tapeSize = RECOMMENDED_TAPE_SIZE,
			int stackSize = RECOMMENDED_STACK_SIZE) {
			var interpreter = Interpreter
				.GrabHosted (tapeSize, stackSize)
				.FeedInput (Input)
				.Feed (code);
			interpreter.Interpret ();
			return interpreter.GetState ();
		}

		public async Task<QoState> InterpretAsync (string code) {
			return await Task.Factory.StartNew<QoState> (() => Interpret (code));
		}
	}
}

