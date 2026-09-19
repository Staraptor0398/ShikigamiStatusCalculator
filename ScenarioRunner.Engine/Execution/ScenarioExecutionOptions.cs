namespace ScenarioRunner.Execution
{
	public class ScenarioExecutionOptions
	{
		public bool WatchMode { get; }
		public bool CleanupGuiOnExit { get; }

		public ScenarioExecutionOptions(bool watchMode, bool cleanupGuiOnExit = false)
		{
			WatchMode = watchMode;
			CleanupGuiOnExit = cleanupGuiOnExit;
		}
	}
}
