using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
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

		private GuiSession mResultViewSession;
		private Window mResultViewForm;
		private AutomationElementMap mResultViewElementMap;

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

			resetResultViewCache();

			AutomationElement detailButton = mGuiOperator.GetMainElement(session, AutomationIds.MainForm.CALC_DETAIL, ControlType.Button);

			mButtonOperator.Click(detailButton);
		}

		public void Check(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			AutomationElementMap elementMap = getResultViewElementMap(session);

			AutomationElement mitamaStatusElement = elementMap.Get(AutomationIds.ResultViewForm.MITAMA_STATUS);

			string mitamaStatus = mTextBoxOperator.GetText(mitamaStatusElement);

			if (string.IsNullOrWhiteSpace(mitamaStatus))
			{
				throw new InvalidOperationException("Calculation detail for Mitama-only status is empty.");
			}

			AutomationElement finalStatusElement = elementMap.Get(AutomationIds.ResultViewForm.FINAL_STATUS);

			string finalStatus = mTextBoxOperator.GetText(finalStatusElement);

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
			AutomationElementMap elementMap = getResultViewElementMap(session);

			AutomationElement closeButton = elementMap.Get(AutomationIds.ResultViewForm.CLOSE, ControlType.Button);

			mButtonOperator.Click(closeButton);

			mWindowWaiter.WaitForWindowClosed(resultViewForm);

			resetResultViewCache();
		}

		private Window getResultViewForm(GuiSession session)
		{
			if (ReferenceEquals(mResultViewSession, session) && mResultViewForm != null)
			{
				return mResultViewForm;
			}

			int processId = session.Application.ProcessId;

			mResultViewForm = mWindowWaiter.WaitForWindow(session, element => element.Properties.ProcessId.ValueOrDefault == processId && element.Properties.AutomationId.ValueOrDefault == AutomationIds.ResultViewForm.ID);
			mResultViewSession = session;
			mResultViewElementMap = null;

			return mResultViewForm;
		}

		private AutomationElementMap getResultViewElementMap(GuiSession session)
		{
			Window resultViewForm = getResultViewForm(session);

			if (mResultViewElementMap == null)
			{
				mResultViewElementMap = new AutomationElementMap(resultViewForm);
			}

			return mResultViewElementMap;
		}

		private void resetResultViewCache()
		{
			mResultViewElementMap = null;
			mResultViewForm = null;
			mResultViewSession = null;
		}
	}
}
