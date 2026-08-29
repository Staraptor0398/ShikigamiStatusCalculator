using FlaUI.Core.AutomationElements;
using ScenarioRunner.Automation.Definition;
using System;

namespace ScenarioRunner.Automation.Operator
{
	public class ShikigamiOperator
	{
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
			mComboBoxOperator.SelectItem(mainWindow, AutomationIds.MainForm.SHIKIGAMI, shikigamiName);
		}

		public void SelectFirst(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			Window mainWindow = mGuiOperator.GetMainWindow(session);
			mComboBoxOperator.SelectFirstItem(mainWindow, AutomationIds.MainForm.SHIKIGAMI);
		}

		public void Reload(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			Window mainWindow = mGuiOperator.GetMainWindow(session);
			mButtonOperator.Click(mainWindow, AutomationIds.MainForm.RELOAD_SHIKIGAMI);
		}

		public void SaveSelectedShikigamiWithoutChanges(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			Window registerForm = openEditForm(session);
			mButtonOperator.Click(registerForm, AutomationIds.ShikigamiRegisterForm.REGISTER);
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

			if (!mComboBoxOperator.CanSelectFirstItem(mainWindow, AutomationIds.MainForm.SHIKIGAMI))
			{
				throw new InvalidOperationException("Shikigami ComboBox has no items.");
			}
		}

		private Window openEditForm(GuiSession session)
		{
			Window mainWindow = mGuiOperator.GetMainWindow(session);
			mButtonOperator.Click(mainWindow, AutomationIds.MainForm.EDIT_SHIKIGAMI);

			int processId = session.Application.ProcessId;

			return mWindowOperator.WaitForWindow(session, element => element.Properties.ProcessId.Value == processId && element.AutomationId == AutomationIds.ShikigamiRegisterForm.ID);
		}
	}
}
