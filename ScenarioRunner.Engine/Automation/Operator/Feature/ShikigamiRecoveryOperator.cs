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

		private readonly WindowWaiter mWindowWaiter;

		public ShikigamiRecoveryOperator()
		{
			mButtonOperator = new ButtonOperator();
			mFileDialogOperator = new FileDialogOperator();
			mGuiOperator = new GuiOperator();

			mWindowWaiter = new WindowWaiter();
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

			Window mainWindow = mGuiOperator.GetMainWindow(session);

			AutomationElement recoveryButton = mGuiOperator.GetMainElement(session, AutomationIds.MainForm.SHIKIGAMI_RECOVERY, ControlType.Button);

			mButtonOperator.Click(recoveryButton);

			FileDialogElements fileDialogElements = mWindowWaiter.WaitForFileDialog(session, mainWindow);
			mFileDialogOperator.SelectLoadFile(fileDialogElements, recoveryFilePath);

			Window recoveryForm = getRecoveryForm(session);
			mButtonOperator.Click(recoveryForm, AutomationIds.ShikigamiRecoveryDialog.RECOVERY);
		}

		private Window getRecoveryForm(GuiSession session)
		{
			Window mainWindow = mGuiOperator.GetMainWindow(session);

			return mWindowWaiter.WaitForProcessWindow(session, mainWindow, element => element.Properties.AutomationId.ValueOrDefault == AutomationIds.ShikigamiRecoveryDialog.ID);
		}
	}
}
