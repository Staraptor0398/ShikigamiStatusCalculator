using System;

namespace ScenarioRunnerCli
{
	internal static class Program
	{
		private const int EXIT_SUCCESS = 0;
		private const int EXIT_SCENARIO_FAILED = 1;
		private const int EXIT_INVALID_ARGUMENT = 2;

		[STAThread]
		private static int Main(string[] args)
		{
			try
			{
				CommandLineOptions options = CommandLineOptions.Parse(args);

				if (options.ShowHelp)
				{
					printUsage();
					return EXIT_SUCCESS;
				}

				bool isSuccess;

				if (!string.IsNullOrWhiteSpace(options.ScenarioPath))
				{
					var runner = new ScenarioCliRunner();
					isSuccess = runner.Run(options.ScenarioPath, options.GuiExecutablePath);
				}
				else
				{
					var runner = new ScenarioBatchRunner();
					isSuccess = runner.Run(options.ScenarioDirectoryPath, options.GuiExecutablePath);
				}

				return isSuccess ? EXIT_SUCCESS : EXIT_SCENARIO_FAILED;
			}
			catch (ArgumentException ex)
			{
				Console.Error.WriteLine(ex.Message);
				Console.Error.WriteLine();
				printUsage();
				return EXIT_INVALID_ARGUMENT;
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine(ex.Message);
				return EXIT_SCENARIO_FAILED;
			}
		}

		private static void printUsage()
		{
			Console.WriteLine("ScenarioRunnerCli");
			Console.WriteLine();
			Console.WriteLine("Usage:");
			Console.WriteLine("  ScenarioRunnerCli.exe --run <scenario path> [--gui-path <Gui.exe path>]");
			Console.WriteLine("  ScenarioRunnerCli.exe --run-directory <directory path> [--gui-path <Gui.exe path>]");
			Console.WriteLine();
			Console.WriteLine("Options:");
			Console.WriteLine("  --run <path>            Scenario file to execute.");
			Console.WriteLine("  --run-directory <path>  Directory containing Scenario files to execute.");
			Console.WriteLine("  --gui-path <path>       Optional explicit path to Gui.exe.");
			Console.WriteLine("  --help, -h              Show this help.");
		}
	}
}
