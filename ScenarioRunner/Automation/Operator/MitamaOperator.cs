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

		public MitamaOperator()
		{
			mButtonOperator = new ButtonOperator();
			mComboBoxOperator = new ComboBoxOperator();
			mFileDialogOperator = new FileDialogOperator();
			mGuiOperator = new GuiOperator();
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

			Window mainWindow = mGuiOperator.GetMainWindow(context.GuiSession);

			string resolvedPath = context.ResolvePath(filePath);

			mButtonOperator.Click(mainWindow, MAIN_LOAD_BUTTON_AUTOMATION_ID);

			Window loadDialog = getLoadDialog(context.GuiSession);

			mComboBoxOperator.SelectItem(mainWindow, LOAD_TYPE_COMBO_BOX_AUTOMATION_ID, MITAMA_SET_LOAD_TYPE);

			mButtonOperator.Click(loadDialog, BROWSE_BUTTON_AUTOMATION_ID);

			Window fileDialog = getFileDialog(context.GuiSession);
			mFileDialogOperator.SelectFile(fileDialog, resolvedPath);

			mButtonOperator.Click(loadDialog, LOAD_BUTTON_AUTOMATION_ID);
		}

		private Window getLoadDialog(GuiSession session)
		{
			Window[] windows = session.Application.GetAllTopLevelWindows(session.Automation);

			foreach (Window window in windows)
			{
				var browseButton = window.FindFirstDescendant(
					cf => cf.ByAutomationId(BROWSE_BUTTON_AUTOMATION_ID));

				if (browseButton != null)
				{
					return window;
				}
			}

			throw new InvalidOperationException("SaveData load dialog was not found.");
		}

		private Window getFileDialog(GuiSession session)
		{
			Window[] windows = session.Application.GetAllTopLevelWindows(session.Automation);

			foreach (Window window in windows)
			{
				var openButton = window.FindFirstDescendant(cf => cf.ByAutomationId("1"));

				if (openButton != null)
				{
					return window;
				}
			}

			throw new InvalidOperationException("OpenFileDialog was not found.");
		}
	}
}
