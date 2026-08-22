using ScenarioRunner.Automation.Operator;
using ScenarioRunner.ScenarioFormat;
using System;

namespace ScenarioRunner.Execution
{
	public class ScenarioCommandExecutor
	{
		private readonly GuiOperator mGuiOperator;

		public ScenarioCommandExecutor(string guiExecutablePath)
		{
			mGuiOperator = new GuiOperator(guiExecutablePath);
		}

		public void Execute(ScenarioStep step, ScenarioExecutonContext context)
		{
			switch (step.CommandType)
			{
				case ScenarioCommandType.OPEN_GUI:
					mGuiOperator.Open(context);
					return;
				case ScenarioCommandType.CLOSE_GUI:
					mGuiOperator.Close(context);
					return;
				case ScenarioCommandType.SELECT_SHIKIGAMI:
				case ScenarioCommandType.LOAD_MITAMA:
				case ScenarioCommandType.CALCULATE:
				case ScenarioCommandType.CLEAR:
				case ScenarioCommandType.RELOAD_SHIKIGAMI:
				case ScenarioCommandType.BREAK_SHIKIGAMI_HEADER:
				case ScenarioCommandType.CHECK_CALCULATION:
				case ScenarioCommandType.CHECK_SHIKIGAMI:
					return;

				default:
					throw new ArgumentOutOfRangeException(nameof(step.CommandType), step.CommandType, null);
			}
		}
	}
}
