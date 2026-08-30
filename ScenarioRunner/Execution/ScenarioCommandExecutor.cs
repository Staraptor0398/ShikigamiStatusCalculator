using FlaUI.Core.AutomationElements;
using ScenarioRunner.Automation.Definition;
using ScenarioRunner.Automation.Operator;
using ScenarioRunner.Automation.Waiter;
using ScenarioRunner.ScenarioFormat;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ScenarioRunner.Execution
{
	public class ScenarioCommandExecutor
	{
		private readonly WindowOperator mWindowOperator;
		private readonly GuiOperator mGuiOperator;
		private readonly ShikigamiOperator mShikigamiOperator;
		private readonly CalculationOperator mCalculationOperator;
		private readonly MitamaOperator mMitamaOperator;
		private readonly DialogOperator mDialogOperator;
		private readonly ShikigamiDataOperator mShikigamiDataOperator;
		private readonly InputOperator mInputOperator;
		private readonly ShikigamiRecoveryOperator mShikigamiRecoveryOperator;

		private readonly ShikigamiDataWaiter mShikigamiDataWaiter;
		private readonly WindowWaiter mWindowWaiter;

		public ScenarioCommandExecutor()
		{
			mWindowOperator = new WindowOperator();
			mGuiOperator = new GuiOperator();
			mShikigamiOperator = new ShikigamiOperator();
			mCalculationOperator = new CalculationOperator();
			mMitamaOperator = new MitamaOperator();
			mDialogOperator = new DialogOperator();
			mShikigamiDataOperator = new ShikigamiDataOperator();
			mInputOperator = new InputOperator();
			mShikigamiRecoveryOperator = new ShikigamiRecoveryOperator();

			mShikigamiDataWaiter = new ShikigamiDataWaiter();
			mWindowWaiter = new WindowWaiter();
		}

		public void Execute(ScenarioStep step, ScenarioExecutonContext context)
		{
			switch (step.CommandType)
			{
				case ScenarioCommandType.LAUNCH_GUI:
					mGuiOperator.Launch(context);

					startGuiWindowArrangement(context);
					return;
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
				case ScenarioCommandType.EQUIP_MITAMA:
					mInputOperator.Equip(context.GuiSession, step.Arguments);
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
					return;
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

		private void startGuiWindowArrangement(ScenarioExecutonContext context)
		{
			int processId = context.GuiSession.Application.ProcessId;

			Task.Run(() =>
			{
				Window window = mWindowWaiter.WaitForWindow(context.GuiSession, element => element.Properties.ProcessId.ValueOrDefault == processId && element.Properties.AutomationId.ValueOrDefault == AutomationIds.MainForm.ID, CancellationToken.None);

				mWindowOperator.SetBounds(window, context.GuiBounds);
			});
		}

	}
}
