namespace ScenarioRunner.Execution
{
	public class ScenarioExecutionOptions
	{
		public bool WatchMode { get; }
		public bool CleanupGuiOnExit { get; }
		public bool ArrangeGuiWindow { get; }

		public ScenarioExecutionOptions(bool watchMode, bool cleanupGuiOnExit = false, bool arrangeGuiWindow = true)
		{
			WatchMode = watchMode;
			CleanupGuiOnExit = cleanupGuiOnExit;
			ArrangeGuiWindow = arrangeGuiWindow;
		}
	}
}
