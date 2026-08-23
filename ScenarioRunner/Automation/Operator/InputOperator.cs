using FlaUI.Core.AutomationElements;
using System;

namespace ScenarioRunner.Automation.Operator
{
	public class InputOperator
	{
		private const string CLEAR_BUTTON_AUTOMATION_ID = "btnClear";
		private const string YES_BUTTON_AUTOMATION_ID = "6";

		private readonly ButtonOperator mButtonOperator;
		private readonly DialogOperator mDialogOperator;
		private readonly GuiOperator mGuiOperator;

		public InputOperator()
		{
			mButtonOperator = new ButtonOperator();
			mDialogOperator = new DialogOperator();
			mGuiOperator = new GuiOperator();
		}

		public void Clear(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			Window mainWindow = mGuiOperator.GetMainWindow(session);

			mButtonOperator.Click(mainWindow, CLEAR_BUTTON_AUTOMATION_ID);

			Window dialog = mDialogOperator.GetActiveDialog(session);

			if (dialog == null)
			{
				throw new InvalidOperationException("Clear confirmation dialog was not found.");
			}

			mButtonOperator.Click(dialog, YES_BUTTON_AUTOMATION_ID);
		}
	}
}
