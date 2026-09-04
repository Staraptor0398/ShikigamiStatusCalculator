using System;

namespace ScenarioRunner.ScenarioFormat
{
	public class ScenarioValidator
	{
		public void Validate(Scenario scenario)
		{
			if (scenario == null)
			{
				throw new ScenarioValidationException("Scenario is null.");
			}

			validateBoundary(scenario);
			validateStepRange(scenario);
			validateArguments(scenario);
		}

		private void validateBoundary(Scenario scenario)
		{
			if (scenario.StartLine == -1)
			{
				throw new ScenarioValidationException("START is not defined.");
			}
			if (scenario.EndLine == -1)
			{
				throw new ScenarioValidationException("END is not defined.");
			}

			if (scenario.StartLine > scenario.EndLine)
			{
				throw new ScenarioValidationException("END is defined before START.");
			}
		}

		private void validateStepRange(Scenario scenario)
		{
			foreach (ScenarioStep step in scenario.Steps)
			{
				if (step.LineNumber < scenario.StartLine)
				{
					throw new ScenarioValidationException($"Command is defined before START at line {step.LineNumber}: {step.RawText}");
				}

				if (step.LineNumber > scenario.EndLine)
				{
					throw new ScenarioValidationException($"Command is defined after END at line {step.LineNumber}: {step.RawText}");
				}
			}
		}

		private void validateArguments(Scenario scenario)
		{
			foreach (ScenarioStep step in scenario.Steps)
			{
				if (step.CommandType == ScenarioCommandType.EQUIP_MITAMA)
				{
					validateEquipMitamaArguments(step);
					continue;
				}

				int expectedCount = getExpectedArgumentCount(step.CommandType);

				if (step.Arguments.Count < expectedCount)
				{
					throw new ScenarioValidationException($"Argument is missing at line {step.LineNumber}: {step.RawText}");
				}

				if (step.Arguments.Count > expectedCount)
				{
					throw new ScenarioValidationException($"Too many arguments at line {step.LineNumber}: {step.RawText}");
				}
			}
		}

		private void validateEquipMitamaArguments(ScenarioStep step)
		{
			if (step.Arguments.Count == 0)
			{
				throw new ScenarioValidationException($"Argument is missing at line {step.LineNumber}: {step.RawText}");
			}

			switch (step.Arguments[0])
			{
				case "MAIN":
					validateArgumentCount(step, 3, 3);
					break;
				case "SUB":
					validateArgumentCount(step, 3, 5);
					break;
				case "SET":
					validateArgumentCount(step, 3, 3);
					break;
				case "UNIQUE":
					validateArgumentCount(step, 3, 3);
					break;
				default:
					throw new ScenarioValidationException($"Unknown EQUIP MITAMA target at line {step.LineNumber}: {step.RawText}");
			}
		}

		private void validateArgumentCount(ScenarioStep step, int minimumCount, int maximumCount)
		{
			if (step.Arguments.Count < minimumCount)
			{
				throw new ScenarioValidationException($"Argument is missing at line {step.LineNumber}: {step.RawText}");
			}

			if (step.Arguments.Count > maximumCount)
			{
				throw new ScenarioValidationException($"Too many arguments at line {step.LineNumber}: {step.RawText}");
			}
		}

		private int getExpectedArgumentCount(ScenarioCommandType commandType)
		{
			switch (commandType)
			{
				case ScenarioCommandType.LAUNCH_GUI:
				case ScenarioCommandType.OPEN_GUI:
				case ScenarioCommandType.CLOSE_GUI:
				case ScenarioCommandType.CLOSE_DIALOG:
				case ScenarioCommandType.CALCULATE:
				case ScenarioCommandType.CLEAR:
				case ScenarioCommandType.RELOAD_SHIKIGAMI:
				case ScenarioCommandType.BREAK_SHIKIGAMI_HEADER:
				case ScenarioCommandType.CREATE_SHIKIGAMI_BACKUP:
				case ScenarioCommandType.CHECK_CALCULATION:
				case ScenarioCommandType.CHECK_SHIKIGAMI:
				case ScenarioCommandType.WAIT_SHIKIGAMI_AUTO_REPAIR:
					return 0;
				case ScenarioCommandType.SELECT_SHIKIGAMI:
				case ScenarioCommandType.LOAD_MITAMA:
				case ScenarioCommandType.CHECK_DIALOG:
				case ScenarioCommandType.REMOVE_SHIKIGAMI:
				case ScenarioCommandType.RECOVER_SHIKIGAMI:
					return 1;
				default:
					throw new ArgumentOutOfRangeException(nameof(commandType), commandType, null);
			}
		}
	}
}
