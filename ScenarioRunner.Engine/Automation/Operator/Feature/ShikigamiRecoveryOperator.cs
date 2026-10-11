using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using ScenarioRunner.Automation.Definition;
using ScenarioRunner.Automation.Waiter;
using System;

namespace ScenarioRunner.Automation.Operator.Feature
{
	public class ShikigamiRecoveryOperator
	{
		private readonly ButtonOperator mButtonOperator;
		private readonly FileDialogOperator mFileDialogOperator;
		private readonly GuiOperator mGuiOperator;

		private readonly ProcessWindowWaiter mProcessWindowWaiter;
		private readonly FileDialogWaiter mFileDialogWaiter;

		public ShikigamiRecoveryOperator()
		{
			mButtonOperator = new ButtonOperator();
			mFileDialogOperator = new FileDialogOperator();
			mGuiOperator = new GuiOperator();

			mProcessWindowWaiter = new ProcessWindowWaiter();
			mFileDialogWaiter = new FileDialogWaiter();
		}

		public void Recover(GuiSession session, string recoveryFilePath)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			if (string.IsNullOrWhiteSpace(recoveryFilePath))
			{
				throw new ArgumentException("Recovery file path is empty.", nameof(recoveryFilePath));
			}

			string stage = "OpenRecoveryFileDialog";

			try
			{
				Window mainWindow = mGuiOperator.GetMainWindow(session);

				AutomationElement recoveryButton = mGuiOperator.GetMainElement(session, AutomationIds.MainForm.SHIKIGAMI_RECOVERY, ControlType.Button);

				mButtonOperator.PostClick(recoveryButton);

				stage = "SelectRecoveryFile";

				FileDialogElements fileDialogElements = mFileDialogWaiter.WaitForFileDialog(session, mainWindow);

				mFileDialogOperator.SelectLoadFile(fileDialogElements, recoveryFilePath);

				stage = "WaitForRecoveryForm";

				Window recoveryForm = getRecoveryForm(session);

				stage = "ExecuteRecovery";

				mButtonOperator.Click(recoveryForm, AutomationIds.ShikigamiRecoveryDialog.RECOVERY);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"ShikigamiRecoveryOperator.Recover failed at stage '{stage}': " + $"RecoveryFilePath={recoveryFilePath}. " + $"{ex.Message}", ex);
			}
		}

		private Window getRecoveryForm(GuiSession session)
		{
			Window mainWindow = mGuiOperator.GetMainWindow(session);

			return mProcessWindowWaiter.WaitForProcessWindow(session, mainWindow, element => element.Properties.AutomationId.ValueOrDefault == AutomationIds.ShikigamiRecoveryDialog.ID);
		}
	}
}
