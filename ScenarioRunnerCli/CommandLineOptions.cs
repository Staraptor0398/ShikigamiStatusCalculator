using System;

namespace ScenarioRunnerCli
{
	public class CommandLineOptions
	{
		public string ScenarioPath { get; }
		public string ScenarioDirectoryPath { get; }
		public string GuiExecutablePath { get; }
		public bool ShowHelp { get; }

		private CommandLineOptions(string scenarioPath, string scenarioDirectoryPath, string guiExecutablePath, bool showHelp)
		{
			ScenarioPath = scenarioPath;
			ScenarioDirectoryPath = scenarioDirectoryPath;
			GuiExecutablePath = guiExecutablePath;
			ShowHelp = showHelp;
		}

		public static CommandLineOptions Parse(string[] args)
		{
			if (args == null)
			{
				throw new ArgumentNullException(nameof(args));
			}

			string scenarioPath = null;
			string scenarioDirectoryPath = null;
			string guiExecutablePath = null;
			bool showHelp = false;

			for (int i = 0; i < args.Length; i++)
			{
				switch (args[i])
				{
					case "--run":
						scenarioPath = readValue(args, ref i, "--run");
						break;

					case "--run-directory":
						scenarioDirectoryPath = readValue(args, ref i, "--run-directory");
						break;

					case "--gui-path":
						guiExecutablePath = readValue(args, ref i, "--gui-path");
						break;

					case "--help":
					case "-h":
						showHelp = true;
						break;

					default:
						throw new ArgumentException($"Unknown argument: {args[i]}");
				}
			}

			if (!showHelp)
			{
				bool hasScenarioPath = !string.IsNullOrWhiteSpace(scenarioPath);
				bool hasScenarioDirectoryPath = !string.IsNullOrWhiteSpace(scenarioDirectoryPath);

				if (!hasScenarioPath && !hasScenarioDirectoryPath)
				{
					throw new ArgumentException("Either --run or --run-directory is required.");
				}

				if (hasScenarioPath && hasScenarioDirectoryPath)
				{
					throw new ArgumentException("--run and --run-directory cannot be used together.");
				}
			}

			return new CommandLineOptions(scenarioPath, scenarioDirectoryPath, guiExecutablePath, showHelp);
		}

		private static string readValue(string[] args, ref int index, string optionName)
		{
			int valueIndex = index + 1;

			if (valueIndex >= args.Length || string.IsNullOrWhiteSpace(args[valueIndex]))
			{
				throw new ArgumentException($"A value is required for {optionName}.");
			}

			index = valueIndex;
			return args[valueIndex];
		}
	}
}
