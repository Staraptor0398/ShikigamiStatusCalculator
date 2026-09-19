using FlaUI.Core.AutomationElements;
using ScenarioRunner.Automation.Definition;
using ScenarioRunner.Automation.Waiter;
using ScenarioRunner.Execution;
using System;
using System.Threading;

namespace ScenarioRunner.Automation.Operator.Feature
{
	public class MitamaOperator
	{
		private const string MITAMA_SET_LOAD_TYPE = "御魂セット保存データ";

		private const int FILE_PATH_WAIT_TIMEOUT_MS = 5000;
		private const int FILE_PATH_WAIT_INTERVAL_MS = 100;

		private readonly ButtonOperator mButtonOperator;
		private readonly ComboBoxOperator mComboBoxOperator;
		private readonly TextBoxOperator mTextBoxOperator;
		private readonly FileDialogOperator mFileDialogOperator;
		private readonly GuiOperator mGuiOperator;

		private readonly WindowWaiter mWindowWaiter;

		public MitamaOperator()
		{
			mButtonOperator = new ButtonOperator();
			mComboBoxOperator = new ComboBoxOperator();
			mTextBoxOperator = new TextBoxOperator();
			mFileDialogOperator = new FileDialogOperator();
			mGuiOperator = new GuiOperator();

			mWindowWaiter = new WindowWaiter();
		}

		public void Load(ScenarioExecutonContext context, string filePath)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			if (context.GuiSession == null)
			{
				throw new InvalidOperationException("Gui.exe is not running.");
			}

			GuiSession session = context.GuiSession;
			int processId = session.Application.ProcessId;

			Window mainWindow = mGuiOperator.GetMainWindow(session);
			string resolvedPath = context.ResolvePath(filePath);

			mButtonOperator.Click(mainWindow, AutomationIds.MainForm.LOAD);

			Window loadDialog = getLoadDialog(session, processId);

			mComboBoxOperator.SelectItem(loadDialog, AutomationIds.SaveDataLoadDialog.LOAD_TYPE, MITAMA_SET_LOAD_TYPE);

			string selectedLoadType = mComboBoxOperator.GetValue(loadDialog, AutomationIds.SaveDataLoadDialog.LOAD_TYPE);

			if (!string.Equals(selectedLoadType, MITAMA_SET_LOAD_TYPE, StringComparison.Ordinal))
			{
				throw new InvalidOperationException(
					$"SaveData load type was not selected correctly. Expected={MITAMA_SET_LOAD_TYPE}, Actual={selectedLoadType}");
			}

			mButtonOperator.Click(loadDialog, AutomationIds.SaveDataLoadDialog.BROWSE);

			Window fileDialog = mWindowWaiter.WaitForFileDialog(session);
			mFileDialogOperator.SelectFile(fileDialog, resolvedPath);

			waitForFilePath(loadDialog, resolvedPath);

			mButtonOperator.Click(loadDialog, AutomationIds.SaveDataLoadDialog.LOAD);

			mWindowWaiter.WaitForWindowClosed(session, element => isLoadDialog(element, processId));
		}

		private Window getLoadDialog(GuiSession session, int processId)
		{
			return mWindowWaiter.WaitForWindow(session, element => isLoadDialog(element, processId));
		}

		private void waitForFilePath(Window loadDialog, string expectedFilePath)
		{
			int elapsed = 0;
			string actualFilePath = "";

			while (elapsed < FILE_PATH_WAIT_TIMEOUT_MS)
			{
				actualFilePath = mTextBoxOperator.GetText(loadDialog, AutomationIds.SaveDataLoadDialog.FILE_PATH);

				if (string.Equals(actualFilePath, expectedFilePath, StringComparison.OrdinalIgnoreCase))
				{
					return;
				}

				Thread.Sleep(FILE_PATH_WAIT_INTERVAL_MS);
				elapsed += FILE_PATH_WAIT_INTERVAL_MS;
			}

			throw new InvalidOperationException(
				$"SaveData file path was not selected correctly within {FILE_PATH_WAIT_TIMEOUT_MS} ms. Expected={expectedFilePath}, Actual={actualFilePath}");
		}

		private bool isLoadDialog(AutomationElement element, int processId)
		{
			return
				element.Properties.ProcessId.ValueOrDefault == processId &&
				element.FindFirstDescendant(cf => cf.ByAutomationId(AutomationIds.SaveDataLoadDialog.BROWSE)) != null;
		}
	}
}
