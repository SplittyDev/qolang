using System;
using System.IO;
using System.Reflection;
using Codeaddicts.libArgument;

/*
 * GitHub:
 * -> https://github.com/splittydev/qolang
 * 
 * Specification:
 * -> https://goo.gl/7KSvUK
 */

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
			if (string.IsNullOrEmpty (options.input) && !options.input_stdin)
				options.help = true;

			// Print help if requested
			if (options.help) {
				PrintVersion ();
				ArgumentParser.Help ();
				return;
			}

			string source = string.Empty;

			// Check if source should be read from file
			if (!string.IsNullOrEmpty (options.input)) {

				// Check if file exists
				if (!File.Exists (options.input)) {
					Console.Error.WriteLine ("File not found: {0}", options.input);
					return;
				}

				// Read source from file
				source = File.ReadAllText (options.input);
			}

			// Check if source should be read from stdin
			if (options.input_stdin) {

				Console.WriteLine ("Reading source...");

				// Read source from stdin
				var stdin = Console.OpenStandardInput ();
				using (var reader = new StreamReader (stdin)) {
					source = reader.ReadToEnd ();
				}
			}

			// Interpret source
			Interpreter
				.GrabNew (options.MemorySize, options.StackSize)
				.Feed (source)
				.Wrap (options.wrap)
				.Interpret ();
		}

		static void PrintVersion () {
			var version = Assembly.GetEntryAssembly ().GetName ().Version;
			Console.WriteLine ("qo Interpreter v{0}\n", version);
		}
	}
}
