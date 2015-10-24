using System;
using System.Reflection;
using Codeaddicts.libArgument;

namespace qo
{
	class MainClass
	{
		public static void Main (string[] args) {

			// Parse arguments
			var options = ArgumentParser.Parse<Options> (args);

			// Print version if requested
			if (options.version) {
				PrintVersion ();
				return;
			}

			// Check if input is set
			if (!string.IsNullOrEmpty (options.input) || !options.input_stdin)
				options.help = true;

			// Print help if requested
			if (options.help) {
				PrintVersion ();
				ArgumentParser.Help ();
				return;
			}
		}

		static void PrintVersion () {
			var version = Assembly.GetEntryAssembly ().GetName ().Version;
			Console.WriteLine ("qo Interpreter v{0}\n", version);
		}
	}
}
