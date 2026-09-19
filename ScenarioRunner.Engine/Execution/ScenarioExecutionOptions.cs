namespace ScenarioRunner.Execution
{
	public class ScenarioExecutionOptions
	{
		public bool WatchMode { get; }

		public ScenarioExecutionOptions(bool watchMode)
		{
			WatchMode = watchMode;
		}
	}
}
