using System;
using System.IO;
using System.Reflection;
using Codeaddicts.libArgument;

/*
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

			// Read from stdin if no file is given
			if (string.IsNullOrEmpty (options.input)) {
				
				// Read source from stdin
				var stdin = Console.OpenStandardInput ();
				using (var reader = new StreamReader (stdin)) {
					source = reader.ReadToEnd ();
				}
			}

			// Check if source should be minified
			if (options.minify) {

				// Minify source
				source = Minifier
					.GrabNew ()
					.Feed (source)
					.Minify ();

				Console.WriteLine (source);
				return;
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
