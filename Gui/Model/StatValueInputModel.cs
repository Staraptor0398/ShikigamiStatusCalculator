namespace Gui.Model
{
	public class StatValueInputModel
	{
		public string Type { get; set; } = string.Empty;

		public string ValueText { get; set; } = string.Empty;

		public double Value => double.Parse(ValueText);
	}
}
