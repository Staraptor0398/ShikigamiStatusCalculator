using FlaUI.Core.AutomationElements;
using System;

namespace ScenarioRunner.Automation.Operator
{
	public class ShikigamiOperator
	{
		private const string SHIKIGAMI_COMBO_BOX_AUTOMATION_ID = "cmbShikigami";
		private const string SHIKIGAMI_RELOAD_BUTTON_AUTOMATION_ID = "btnReLoad";
		private const string SHIKIGAMI_EDIT_BUTTON_AUTOMATION_ID = "btnRegister";
		private const string SHIKIGAMI_REGISTER_FORM_AUTOMATION_ID = "ShikigamiRegisterForm";
		private const string SHIKIGAMI_REGISTER_BUTTON_AUTOMATION_ID = "btnRegister";

		private readonly ComboBoxOperator mComboBoxOperator;
		private readonly ButtonOperator mButtonOperator;
		private readonly DialogOperator mDialogOperator;
		private readonly GuiOperator mGuiOperator;
		private readonly WindowOperator mWindowOperator;

		public ShikigamiOperator()
		{
			mComboBoxOperator = new ComboBoxOperator();
			mButtonOperator = new ButtonOperator();
			mDialogOperator = new DialogOperator();
			mGuiOperator = new GuiOperator();
			mWindowOperator = new WindowOperator();
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

		public void SelectFirst(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			Window mainWindow = mGuiOperator.GetMainWindow(session);
			mComboBoxOperator.SelectFirstItem(mainWindow, SHIKIGAMI_COMBO_BOX_AUTOMATION_ID);
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

		public void SaveSelectedShikigamiWithoutChanges(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			Window registerForm = openEditForm(session);
			mButtonOperator.Click(registerForm, SHIKIGAMI_REGISTER_BUTTON_AUTOMATION_ID);
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

			if (!mComboBoxOperator.CanSelectFirstItem(mainWindow, SHIKIGAMI_COMBO_BOX_AUTOMATION_ID))
			{
				throw new InvalidOperationException("Shikigami ComboBox has no items.");
			}
		}

		private Window openEditForm(GuiSession session)
		{
			Window mainWindow = mGuiOperator.GetMainWindow(session);
			mButtonOperator.Click(mainWindow, SHIKIGAMI_EDIT_BUTTON_AUTOMATION_ID);

			int processId = session.Application.ProcessId;

			return mWindowOperator.WaitForWindow(session, element => element.Properties.ProcessId.Value == processId && element.AutomationId == SHIKIGAMI_REGISTER_FORM_AUTOMATION_ID);
		}
	}
}
