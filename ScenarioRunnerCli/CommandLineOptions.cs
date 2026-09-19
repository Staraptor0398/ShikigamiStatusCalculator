using System;

namespace ScenarioRunnerCli
{
	public class CommandLineOptions
	{
		public string ScenarioPath { get; }
		public string GuiExecutablePath { get; }
		public bool ShowHelp { get; }

		private CommandLineOptions(string scenarioPath, string guiExecutablePath, bool showHelp)
		{
			ScenarioPath = scenarioPath;
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
			string guiExecutablePath = null;
			bool showHelp = false;

			for (int i = 0; i < args.Length; i++)
			{
				switch (args[i])
				{
					case "--run":
						scenarioPath = readValue(args, ref i, "--run");
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

			if (!showHelp && string.IsNullOrWhiteSpace(scenarioPath))
			{
				throw new ArgumentException("--run is required.");
			}

			return new CommandLineOptions(scenarioPath, guiExecutablePath, showHelp);
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
