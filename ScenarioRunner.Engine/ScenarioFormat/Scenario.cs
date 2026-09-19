using System.Collections.Generic;

namespace ScenarioRunner.ScenarioFormat
{
	public class Scenario
	{
		public string FilePath { get; }
		public int StartLine { get; }
		public int EndLine { get; }
		public IReadOnlyList<ScenarioStep> Steps { get; }

		public Scenario(string filePath, int startLine, int endLine, IReadOnlyList<ScenarioStep> steps)
		{
			FilePath = filePath;
			StartLine = startLine;
			EndLine = endLine;
			Steps = steps;
		}
	}
}
