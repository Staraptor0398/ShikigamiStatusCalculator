using FlaUI.Core.AutomationElements;
using ScenarioRunner.Execution;
using System;

namespace ScenarioRunner.Automation.Operator
{
	public class MitamaOperator
	{
		private const string MAIN_LOAD_BUTTON_AUTOMATION_ID = "btnLoad";
		private const string BROWSE_BUTTON_AUTOMATION_ID = "btnBrowse";
		private const string LOAD_BUTTON_AUTOMATION_ID = "btnLoad";
		private const string LOAD_TYPE_COMBO_BOX_AUTOMATION_ID = "cmbLoadType";
		private const string MITAMA_SET_LOAD_TYPE = "御魂セット保存データ";

		private readonly ButtonOperator mButtonOperator;
		private readonly ComboBoxOperator mComboBoxOperator;
		private readonly FileDialogOperator mFileDialogOperator;
		private readonly GuiOperator mGuiOperator;
		private readonly WindowOperator mWindowOperator;

		public MitamaOperator()
		{
			mButtonOperator = new ButtonOperator();
			mComboBoxOperator = new ComboBoxOperator();
			mFileDialogOperator = new FileDialogOperator();
			mGuiOperator = new GuiOperator();
			mWindowOperator = new WindowOperator();
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
			Window mainWindow = mGuiOperator.GetMainWindow(session);
			string resolvedPath = context.ResolvePath(filePath);

			mButtonOperator.Click(mainWindow, MAIN_LOAD_BUTTON_AUTOMATION_ID);

			Window loadDialog = getLoadDialog(session);
			mComboBoxOperator.SelectItem(loadDialog, LOAD_TYPE_COMBO_BOX_AUTOMATION_ID, MITAMA_SET_LOAD_TYPE);

			mButtonOperator.Click(loadDialog, BROWSE_BUTTON_AUTOMATION_ID);

			Window fileDialog = mWindowOperator.WaitForFileDialog(session);
			mFileDialogOperator.SelectFile(fileDialog, resolvedPath);

			mButtonOperator.Click(loadDialog, LOAD_BUTTON_AUTOMATION_ID);
		}

		private Window getLoadDialog(GuiSession session)
		{
			int processId = session.Application.ProcessId;

			return mWindowOperator.WaitForWindow(session, element => element.Properties.ProcessId.Value == processId && element.FindFirstDescendant(cf => cf.ByAutomationId(BROWSE_BUTTON_AUTOMATION_ID)) != null);
		}
	}
}
