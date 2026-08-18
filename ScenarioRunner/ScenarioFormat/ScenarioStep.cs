using System.Collections.Generic;

namespace ScenarioRunner.ScenarioFormat
{
	public class ScenarioStep
	{
		public int LineNumber { get; }
		public ScenarioCommandType CommandType { get; }

		public IReadOnlyList<string> Arguments { get; }
		public string RawText { get; }

		public ScenarioStep(int lineNumber, ScenarioCommandType commandType, IReadOnlyList<string> arguments, string rawText)
		{
			LineNumber = lineNumber;
			CommandType = commandType;
			Arguments = arguments;
			RawText = rawText;
		}
	}
}
