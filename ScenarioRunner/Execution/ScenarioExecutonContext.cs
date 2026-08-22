using ScenarioRunner.Automation;
using System.IO;

namespace ScenarioRunner.Execution
{
	public class ScenarioExecutonContext
	{
		public string ScenarioPath { get; }
		public ScenarioExecutionOptions Options { get; }
		public GuiSession GuiSession { get; set; }

		public ScenarioExecutonContext(string scenarioPath, ScenarioExecutionOptions options)
		{
			ScenarioPath = scenarioPath;
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
