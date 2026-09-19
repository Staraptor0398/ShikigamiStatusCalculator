using System;
using System.IO;

namespace ScenarioRunner.Startup
{
	public static class GuiExecutablePathResolver
	{
		private const string GUI_EXECUTABLE_FILE_NAME = "Gui.exe";

		public static string Resolve(string explicitPath = null)
		{
			if (!string.IsNullOrWhiteSpace(explicitPath))
			{
				return Path.GetFullPath(explicitPath);
			}

			string applicationDirectoryPath = AppDomain.CurrentDomain.BaseDirectory;
			string colocatedPath = Path.Combine(applicationDirectoryPath, GUI_EXECUTABLE_FILE_NAME);

			if (File.Exists(colocatedPath))
			{
				return colocatedPath;
			}

			string repositoryRootPath = findRepositoryRoot(Environment.CurrentDirectory);

			if (repositoryRootPath == null)
			{
				repositoryRootPath = findRepositoryRoot(applicationDirectoryPath);
			}

			if (repositoryRootPath == null)
			{
				return colocatedPath;
			}

			string configurationName = getConfigurationName(applicationDirectoryPath);

			return Path.Combine(
				repositoryRootPath,
				"Gui",
				"bin",
				"x64",
				configurationName,
				GUI_EXECUTABLE_FILE_NAME);
		}

		private static string findRepositoryRoot(string startPath)
		{
			DirectoryInfo directory = new DirectoryInfo(startPath);

			while (directory != null)
			{
				string guiProjectPath = Path.Combine(directory.FullName, "Gui", "Gui.csproj");
				string scenarioRunnerProjectPath = Path.Combine(directory.FullName, "ScenarioRunner", "ScenarioRunner.csproj");

				if (File.Exists(guiProjectPath) && File.Exists(scenarioRunnerProjectPath))
				{
					return directory.FullName;
				}

				directory = directory.Parent;
			}

			return null;
		}

		private static string getConfigurationName(string applicationDirectoryPath)
		{
			string normalizedPath = applicationDirectoryPath.TrimEnd(
				Path.DirectorySeparatorChar,
				Path.AltDirectorySeparatorChar);

			string directoryName = new DirectoryInfo(normalizedPath).Name;

			if (string.Equals(directoryName, "Release", StringComparison.OrdinalIgnoreCase))
			{
				return "Release";
			}

			return "Debug";
		}
	}
}
