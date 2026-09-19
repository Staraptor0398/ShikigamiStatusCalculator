using ScenarioRunner.Automation.Model;

namespace ScenarioRunner.Automation.Layout
{
	public class ScenarioWindowLayout
	{
		private const double LEFT_WIDTH_RATIO = 0.56;
		private const double MONITOR_HEIGHT_RATIO = 0.625;

		public WindowBounds RunnerBounds { get; }
		public WindowBounds MonitorBounds { get; }
		public WindowBounds GuiBounds { get; }

		public ScenarioWindowLayout(WindowBounds workingArea)
		{
			int leftWidth = (int)(workingArea.Width * LEFT_WIDTH_RATIO);
			int rightWidth = workingArea.Width - leftWidth;

			int monitorHeight = (int)(workingArea.Height * MONITOR_HEIGHT_RATIO);
			int guiHeight = workingArea.Height - monitorHeight;

			MonitorBounds = new WindowBounds(workingArea.X, workingArea.Y, leftWidth, monitorHeight);

			GuiBounds = new WindowBounds(workingArea.X, workingArea.Y + monitorHeight, leftWidth, guiHeight);

			RunnerBounds = new WindowBounds(workingArea.X + leftWidth, workingArea.Y, rightWidth, workingArea.Height);
		}
	}
}
