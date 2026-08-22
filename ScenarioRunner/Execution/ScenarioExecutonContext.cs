using ScenarioRunner.Automation;

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
	}
}
