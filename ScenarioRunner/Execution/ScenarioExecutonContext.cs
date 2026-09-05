using ScenarioRunner.Automation;
using ScenarioRunner.Automation.Model;
using System.IO;
using System.Threading;

namespace ScenarioRunner.Execution
{
	public class ScenarioExecutonContext
	{
		public string ScenarioPath { get; }
		public string GuiExecutablePath { get; }
		public ScenarioExecutionOptions Options { get; }

		public WindowBounds GuiBounds { get; }

		public GuiSession GuiSession { get; set; }

		public string ShikigamiDataFilePath { get; set; }

		public string ShikigamiBrokenDataFilePath { get; set; }
		public string ShikigamiBackupDataFilePath { get; set; }

		public CancellationToken CancellationToken { get; }

		public ScenarioExecutonContext(string scenarioPath, string guiExecutablePath, ScenarioExecutionOptions options, WindowBounds guiBounds, CancellationToken cancellationToken)
		{
			ScenarioPath = scenarioPath;
			GuiExecutablePath = guiExecutablePath;
			Options = options;
			GuiBounds = guiBounds;
			CancellationToken = cancellationToken;
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
