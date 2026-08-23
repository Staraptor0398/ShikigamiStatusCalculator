using System;

namespace ScenarioRunner.ScenarioFormat
{
	public class ScenarioValidator
	{
		public void Validate(Scenario scenario)
		{
			if (scenario == null)
			{
				throw new ArgumentNullException(nameof(scenario));
			}

			validateBoundary(scenario);
			validateStepRange(scenario);
			validateArguments(scenario);
		}

		private void validateBoundary(Scenario scenario)
		{
			if (scenario.StartLine == -1)
			{
				throw new FormatException("START is not defined.");
			}

			if (scenario.EndLine == -1)
			{
				throw new FormatException("END is not defined.");
			}

			if (scenario.StartLine > scenario.EndLine)
			{
				throw new FormatException("END is defined before START.");
			}
		}

		private void validateStepRange(Scenario scenario)
		{
			foreach (ScenarioStep step in scenario.Steps)
			{
				if (step.LineNumber < scenario.StartLine)
				{
					throw new FormatException($"Command is defined before START at line {step.LineNumber}: {step.RawText}");
				}

				if (step.LineNumber > scenario.EndLine)
				{
					throw new FormatException($"Command is defined after END at line {step.LineNumber}: {step.RawText}");
				}
			}
		}

		private void validateArguments(Scenario scenario)
		{
			foreach (ScenarioStep step in scenario.Steps)
			{
				int expectedCount = getExpectedArgumentCount(step.CommandType);

				if (step.Arguments.Count < expectedCount)
				{
					throw new FormatException($"Argument is missing at line {step.LineNumber}: {step.RawText}");
				}

				if (step.Arguments.Count > expectedCount)
				{
					throw new FormatException($"Too many arguments at line {step.LineNumber}: {step.RawText}");
				}
			}
		}

		private int getExpectedArgumentCount(ScenarioCommandType commandType)
		{
			switch (commandType)
			{
				case ScenarioCommandType.OPEN_GUI:
				case ScenarioCommandType.CLOSE_GUI:
				case ScenarioCommandType.CLOSE_DIALOG:
				case ScenarioCommandType.CALCULATE:
				case ScenarioCommandType.CLEAR:
				case ScenarioCommandType.RELOAD_SHIKIGAMI:
				case ScenarioCommandType.BREAK_SHIKIGAMI_HEADER:
				case ScenarioCommandType.CHECK_CALCULATION:
				case ScenarioCommandType.CHECK_SHIKIGAMI:
					return 0;
				case ScenarioCommandType.SELECT_SHIKIGAMI:
				case ScenarioCommandType.LOAD_MITAMA:
				case ScenarioCommandType.CHECK_DIALOG:
				case ScenarioCommandType.REMOVE_SHIKIGAMI:
					return 1;
				default:
					throw new ArgumentOutOfRangeException(nameof(commandType), commandType, null);
			}
		}
	}
}
