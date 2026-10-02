using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using ScenarioRunner.Automation.Definition;
using ScenarioRunner.Automation.Waiter;
using System;

namespace ScenarioRunner.Automation.Operator.Feature
{
	public class ShikigamiEditOperator
	{
		private readonly ButtonOperator mButtonOperator;
		private readonly GuiOperator mGuiOperator;

		private readonly WindowWaiter mWindowWaiter;
		private readonly ProcessWindowWaiter mProcessWindowWaiter;

		private GuiSession mEditSession;
		private Window mEditForm;

		public ShikigamiEditOperator()
		{
			mButtonOperator = new ButtonOperator();
			mGuiOperator = new GuiOperator();

			mWindowWaiter = new WindowWaiter();
			mProcessWindowWaiter = new ProcessWindowWaiter();
		}

		public void Open(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			resetEditFormCache();

			AutomationElement button = mGuiOperator.GetMainElement(session, AutomationIds.MainForm.EDIT_SHIKIGAMI, ControlType.Button);

			mButtonOperator.PostClick(button);

			Window mainWindow = mGuiOperator.GetMainWindow(session);

			mEditForm = mProcessWindowWaiter.WaitForProcessWindow(session, mainWindow, element => element.Properties.AutomationId.ValueOrDefault == AutomationIds.ShikigamiRegisterForm.ID);
			mEditSession = session;
		}

		public void Save(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			Window editForm = getEditForm(session);

			mButtonOperator.Click(editForm, AutomationIds.ShikigamiRegisterForm.REGISTER);

			mWindowWaiter.WaitForWindowClosed(editForm);

			resetEditFormCache();

			if (!session.Application.WaitWhileBusy(TimeSpan.FromSeconds(5)))
			{
				throw new InvalidOperationException("Shikigami update did not complete within 5 seconds.");
			}
		}

		private Window getEditForm(GuiSession session)
		{
			if (ReferenceEquals(mEditSession, session) && mEditForm != null)
			{
				return mEditForm;
			}

			Window mainWindow = mGuiOperator.GetMainWindow(session);

			mEditForm = mProcessWindowWaiter.WaitForProcessWindow(session, mainWindow, element => element.Properties.AutomationId.ValueOrDefault == AutomationIds.ShikigamiRegisterForm.ID);
			mEditSession = session;

			return mEditForm;
		}

		private void resetEditFormCache()
		{
			mEditForm = null;
			mEditSession = null;
		}
	}
}
