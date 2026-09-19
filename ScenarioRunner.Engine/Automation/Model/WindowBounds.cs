namespace ScenarioRunner.Automation.Model
{
	public class WindowBounds
	{
		public int X { get; }
		public int Y { get; }
		public int Width { get; }
		public int Height { get; }

		public WindowBounds(int x, int y, int width, int height)
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
		}
	}
}
