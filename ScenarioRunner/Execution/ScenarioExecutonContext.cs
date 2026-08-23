using ScenarioRunner.Automation;
using System.IO;

namespace ScenarioRunner.Execution
{
	public class ScenarioExecutonContext
	{
		public string ScenarioPath { get; }
		public string GuiExecutablePath { get; }
		public ScenarioExecutionOptions Options { get; }
		public GuiSession GuiSession { get; set; }

		public ScenarioExecutonContext(string scenarioPath, string guiExecutablePath, ScenarioExecutionOptions options)
		{
			ScenarioPath = scenarioPath;
			GuiExecutablePath = guiExecutablePath;
			Options = options;
		}

		public string ResolvePath(string path)
		{
			if (Path.IsPathRooted(path))
			{
				return path;
			}

			string scenarioDirectoryPath = Path.GetDirectoryName(ScenarioPath);

			return Path.GetFullPath(Path.Combine(scenarioDirectoryPath, path));
		}
	}
}
