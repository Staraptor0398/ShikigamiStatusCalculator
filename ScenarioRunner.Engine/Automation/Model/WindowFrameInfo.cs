namespace ScenarioRunner.Automation.Model
{
	public sealed class WindowFrameInfo
	{
		public WindowBounds WindowBounds { get; }
		public WindowBounds DwmFrameBounds { get; }
		public WindowBounds ClientBounds { get; }
		public WindowFrameInsets InvisibleFrame { get; }
		public int WindowBorderWidth { get; }
		public int WindowBorderHeight { get; }

		public WindowFrameInfo(WindowBounds windowBounds, WindowBounds dwmFrameBounds, WindowBounds clientBounds, WindowFrameInsets invisibleFrame, int windowBorderWidth, int windowBorderHeight)
		{
			WindowBounds = windowBounds;
			DwmFrameBounds = dwmFrameBounds;
			ClientBounds = clientBounds;
			InvisibleFrame = invisibleFrame;
			WindowBorderWidth = windowBorderWidth;
			WindowBorderHeight = windowBorderHeight;
		}
	}
}
