using FlaUI.Core.AutomationElements;
using System;

namespace ScenarioRunner.Automation.Operator
{
	public class ShikigamiRecoveryOperator
	{
		private const string SHIKIGAMI_RECOVERY_BUTTON_AUTOMATION_ID = "btnRecoveryShikigami";
		private const string RECOVERY_BUTTON_AUTOMATION_ID = "btnRecovery";

		private readonly ButtonOperator mButtonOperator;
		private readonly FileDialogOperator mFileDialogOperator;
		private readonly GuiOperator mGuiOperator;
		private readonly WindowOperator mWindowOperator;

		public ShikigamiRecoveryOperator()
		{
			mButtonOperator = new ButtonOperator();
			mFileDialogOperator = new FileDialogOperator();
			mGuiOperator = new GuiOperator();
			mWindowOperator = new WindowOperator();
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
			mButtonOperator.Click(mainWindow, SHIKIGAMI_RECOVERY_BUTTON_AUTOMATION_ID);

			Window fileDialog = mWindowOperator.WaitForFileDialog(session);
			mFileDialogOperator.SelectFile(fileDialog, recoveryFilePath);

			Window recoveryForm = getRecoveryForm(session);
			mButtonOperator.Click(recoveryForm, RECOVERY_BUTTON_AUTOMATION_ID);
		}

		private Window getRecoveryForm(GuiSession session)
		{
			int processId = session.Application.ProcessId;

			return mWindowOperator.WaitForWindow(session, element => element.Properties.ProcessId.Value == processId && element.FindFirstDescendant(cf => cf.ByAutomationId(RECOVERY_BUTTON_AUTOMATION_ID)) != null);
		}
	}
}
