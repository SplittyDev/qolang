using System;
using System.Text;
using System.Threading.Tasks;

namespace libqo
{
	public class QoScriptHost
	{
		const int RECOMMENDED_TAPE_SIZE = 30000;
		const int RECOMMENDED_STACK_SIZE = 512;

		readonly Minifier minifier =
			Minifier.GrabNew ();

		public QoScriptHost GrabNew () {
			return new QoScriptHost ();
		}

		public QoScriptHost SetInput (string input) {
			var data = Encoding.ASCII.GetBytes (input);
			Console.OpenStandardInput ().Write (data, 0, data.Length);
			return this;
		}

		public QoState Interpret (string code,
			int tapeSize = RECOMMENDED_TAPE_SIZE,
			int stackSize = RECOMMENDED_STACK_SIZE) {
			var newcode = Minifier
				.GrabNew ()
				.Feed (code)
				.Minify ();
			var interpreter = Interpreter
				.GrabNew (tapeSize, stackSize)
				.Feed (newcode);
			interpreter.Interpret ();
			return interpreter.GetState ();
		}

		public async Task<QoState> InterpretAsync (string code) {
			return await Task.Factory.StartNew<QoState> (() => Interpret (code));
		}
	}
}

