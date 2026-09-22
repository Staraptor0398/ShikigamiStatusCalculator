using FlaUI.Core.AutomationElements;
using ScenarioRunner.Automation.Definition;
using ScenarioRunner.Automation.Waiter;
using System;

namespace ScenarioRunner.Automation.Operator.Feature
{
	public class CalculationDetailOperator
	{
		private readonly ButtonOperator mButtonOperator;
		private readonly TextBoxOperator mTextBoxOperator;
		private readonly GuiOperator mGuiOperator;

		private readonly WindowWaiter mWindowWaiter;

		public CalculationDetailOperator()
		{
			mButtonOperator = new ButtonOperator();
			mTextBoxOperator = new TextBoxOperator();
			mGuiOperator = new GuiOperator();

			mWindowWaiter = new WindowWaiter();
		}

		public void Open(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			Window mainWindow = mGuiOperator.GetMainWindow(session);
			mButtonOperator.Click(mainWindow, AutomationIds.MainForm.CALC_DETAIL);
		}

		public void Check(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			Window resultViewForm = getResultViewForm(session);

			string mitamaStatus = mTextBoxOperator.GetText(resultViewForm, AutomationIds.ResultViewForm.MITAMA_STATUS);

			if (string.IsNullOrWhiteSpace(mitamaStatus))
			{
				throw new InvalidOperationException("Calculation detail for Mitama-only status is empty.");
			}

			string finalStatus = mTextBoxOperator.GetText(resultViewForm, AutomationIds.ResultViewForm.FINAL_STATUS);

			if (string.IsNullOrWhiteSpace(finalStatus))
			{
				throw new InvalidOperationException("Calculation detail for final status is empty.");
			}
		}

		public void Close(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			Window resultViewForm = getResultViewForm(session);

			int processId = session.Application.ProcessId;

			mButtonOperator.Click(resultViewForm, AutomationIds.ResultViewForm.CLOSE);

			mWindowWaiter.WaitForWindowClosed(session, element => element.Properties.ProcessId.ValueOrDefault == processId && element.Properties.AutomationId.ValueOrDefault == AutomationIds.ResultViewForm.ID);
		}

		private Window getResultViewForm(GuiSession session)
		{
			int processId = session.Application.ProcessId;

			return mWindowWaiter.WaitForWindow(session, element => element.Properties.ProcessId.ValueOrDefault == processId && element.Properties.AutomationId.ValueOrDefault == AutomationIds.ResultViewForm.ID);
		}
	}
}
