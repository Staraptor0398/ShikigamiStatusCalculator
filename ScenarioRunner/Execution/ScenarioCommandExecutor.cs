using ScenarioRunner.Automation.Operator;
using ScenarioRunner.Automation.Waiter;
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
		private readonly ShikigamiDataOperator mShikigamiDataOperator;
		private readonly InputOperator mInputOperator;
		private readonly ShikigamiRecoveryOperator mShikigamiRecoveryOperator;

		private readonly ShikigamiDataWaiter mShikigamiDataWaiter;

		public ScenarioCommandExecutor()
		{
			mGuiOperator = new GuiOperator();
			mShikigamiOperator = new ShikigamiOperator();
			mCalculationOperator = new CalculationOperator();
			mMitamaOperator = new MitamaOperator();
			mDialogOperator = new DialogOperator();
			mShikigamiDataOperator = new ShikigamiDataOperator();
			mInputOperator = new InputOperator();
			mShikigamiRecoveryOperator = new ShikigamiRecoveryOperator();

			mShikigamiDataWaiter = new ShikigamiDataWaiter();
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
				case ScenarioCommandType.CLOSE_DIALOG:
					mDialogOperator.Close(context.GuiSession);
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
					mInputOperator.Clear(context.GuiSession);
					return;
				case ScenarioCommandType.RELOAD_SHIKIGAMI:
					mShikigamiOperator.Reload(context.GuiSession);
					return;
				case ScenarioCommandType.BREAK_SHIKIGAMI_HEADER:
					mShikigamiDataOperator.BreakHeader(context);
					return;
				case ScenarioCommandType.REMOVE_SHIKIGAMI:
					mShikigamiDataOperator.RemoveShikigami(context, step.Arguments[0]);
					return;
				case ScenarioCommandType.CREATE_SHIKIGAMI_BACKUP:
					mShikigamiDataOperator.CreateBackup(context.GuiSession);
					return;
				case ScenarioCommandType.RECOVER_SHIKIGAMI:
					string source = step.Arguments[0];

					string recoveryFilePath;

					switch (source)
					{
						case "BROKEN":
							recoveryFilePath = context.ShikigamiBrokenDataFilePath;
							break;

						case "BACKUP":
							recoveryFilePath = context.ShikigamiBackupDataFilePath;
							break;

						default:
							throw new InvalidOperationException($"Unknown recovery source: {source}");
					}

					mShikigamiRecoveryOperator.Recover(context.GuiSession, recoveryFilePath);
					break;
				case ScenarioCommandType.CHECK_CALCULATION:
					mCalculationOperator.Check(context.GuiSession);
					return;
				case ScenarioCommandType.CHECK_SHIKIGAMI:
					mShikigamiOperator.Check(context.GuiSession);
					return;
				case ScenarioCommandType.CHECK_DIALOG:
					mDialogOperator.CheckMessage(context.GuiSession, step.Arguments[0]);
					return;
				case ScenarioCommandType.WAIT_SHIKIGAMI_AUTO_REPAIR:
					mShikigamiDataWaiter.WaitForAutoRepair(context);
					return;
				default:
					throw new ArgumentOutOfRangeException(nameof(step.CommandType), step.CommandType, null);
			}
		}
	}
}
