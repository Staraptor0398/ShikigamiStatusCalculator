namespace ScenarioRunner.Execution
{
	public class ScenarioExecutionOptions
	{
		public bool WatchMode { get; }
		public bool KeepGuiOpenOnFailure { get; }

		public ScenarioExecutionOptions(bool watchMode, bool keepGuiOpenOnFailure)
		{
			WatchMode = watchMode;
			KeepGuiOpenOnFailure = keepGuiOpenOnFailure;
		}
	}
}
