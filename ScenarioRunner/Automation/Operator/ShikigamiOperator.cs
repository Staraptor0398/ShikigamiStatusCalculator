using FlaUI.Core.AutomationElements;
using System;

namespace ScenarioRunner.Automation.Operator
{
	public class ShikigamiOperator
	{
		private const string SHIKIGAMI_COMBO_BOX_AUTOMATION_ID = "cmbShikigami";
		private const string SHIKIGAMI_RELOAD_BUTTON_AUTOMATION_ID = "btnReLoad";

		private readonly ComboBoxOperator mComboBoxOperator;
		private readonly ButtonOperator mButtonOperator;
		private readonly DialogOperator mDialogOperator;
		private readonly GuiOperator mGuiOperator;

		public ShikigamiOperator()
		{
			mComboBoxOperator = new ComboBoxOperator();
			mButtonOperator = new ButtonOperator();
			mDialogOperator = new DialogOperator();
			mGuiOperator = new GuiOperator();
		}

		public void Select(GuiSession session, string shikigamiName)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			Window mainWindow = mGuiOperator.GetMainWindow(session);
			mComboBoxOperator.SelectItem(mainWindow, SHIKIGAMI_COMBO_BOX_AUTOMATION_ID, shikigamiName);
		}

		public void Reload(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			Window mainWindow = mGuiOperator.GetMainWindow(session);
			mButtonOperator.Click(mainWindow, SHIKIGAMI_RELOAD_BUTTON_AUTOMATION_ID);
		}

		public void Check(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			if (mDialogOperator.Exists(session))
			{
				throw new InvalidOperationException("A modal dialog is displayed after reloading shikigami data.");
			}

			Window mainWindow = mGuiOperator.GetMainWindow(session);

			if (mComboBoxOperator.HasItems(mainWindow, SHIKIGAMI_COMBO_BOX_AUTOMATION_ID))
			{
				throw new InvalidOperationException("Shikigami ComboBox has no items.");
			}
		}
	}
}
