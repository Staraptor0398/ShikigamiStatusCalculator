using ScenarioRunner.Automation.Operator;
using ScenarioRunner.ScenarioFormat;
using System;

namespace ScenarioRunner.Execution
{
	public class ScenarioCommandExecutor
	{
		private readonly GuiOperator mGuiOperator;
		private readonly ShikigamiOperator mShikigamiOperator;
		private readonly CalculationOperator mCalculationOperator;
		private readonly MitamaOperator mMitamaOperator;
		private readonly DialogOperator mDialogOperator;

		public ScenarioCommandExecutor(string guiExecutablePath)
		{
			mGuiOperator = new GuiOperator(guiExecutablePath);
			mShikigamiOperator = new ShikigamiOperator();
			mCalculationOperator = new CalculationOperator();
			mMitamaOperator = new MitamaOperator();
			mDialogOperator = new DialogOperator();
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
					mShikigamiOperator.Select(context.GuiSession, step.Arguments[0]);
					return;
				case ScenarioCommandType.LOAD_MITAMA:
					mMitamaOperator.Load(context, step.Arguments[0]);
					return;
				case ScenarioCommandType.CALCULATE:
					mCalculationOperator.Calculate(context.GuiSession);
					return;
				case ScenarioCommandType.CLEAR:
				case ScenarioCommandType.RELOAD_SHIKIGAMI:
				case ScenarioCommandType.BREAK_SHIKIGAMI_HEADER:
					return;
				case ScenarioCommandType.CHECK_CALCULATION:
					mCalculationOperator.Check(context.GuiSession);
					return;
				case ScenarioCommandType.CHECK_SHIKIGAMI:
					return;
				case ScenarioCommandType.CHECK_DIALOG:
					mDialogOperator.CheckMessage(context.GuiSession, step.Arguments[0]);
					return;
				default:
					throw new ArgumentOutOfRangeException(nameof(step.CommandType), step.CommandType, null);
			}
		}
	}
}
