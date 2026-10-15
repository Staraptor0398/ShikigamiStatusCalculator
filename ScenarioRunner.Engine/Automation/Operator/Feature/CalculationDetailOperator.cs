using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using ScenarioRunner.Automation.Definition;
using ScenarioRunner.Automation.Waiter;
using ScenarioRunner.Execution;
using System;

namespace ScenarioRunner.Automation.Operator.Feature
{
	public class CalculationDetailOperator
	{
		private readonly ButtonOperator mButtonOperator;
		private readonly TextBoxOperator mTextBoxOperator;
		private readonly GuiOperator mGuiOperator;

		private readonly WindowWaiter mWindowWaiter;
		private readonly ProcessWindowWaiter mProcessWindowWaiter;

		private GuiSession mResultViewSession;
		private Window mResultViewForm;
		private AutomationElementMap mResultViewElementMap;

		public CalculationDetailOperator()
		{
			mButtonOperator = new ButtonOperator();
			mTextBoxOperator = new TextBoxOperator();
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

			resetResultViewCache();

			AutomationElement detailButton = mGuiOperator.GetMainElement(session, AutomationIds.MainForm.CALC_DETAIL, ControlType.Button);

			mButtonOperator.PostClick(detailButton);
		}

		public void Check(GuiSession session)
		{
			getDetails(session, out _, out _);
		}

		public void Capture(ScenarioExecutonContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			getDetails(context.GuiSession, out string mitamaStatus, out string finalStatus);

			context.CapturedMitamaStatus = mitamaStatus;
			context.CapturedFinalStatus = finalStatus;
		}

		public void CheckUnchanged(ScenarioExecutonContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			if (context.CapturedMitamaStatus == null || context.CapturedFinalStatus == null)
			{
				throw new InvalidOperationException("Calculation detail has not been captured.");
			}

			getDetails(context.GuiSession, out string mitamaStatus, out string finalStatus);

			checkUnchanged("MitamaStatus", context.CapturedMitamaStatus, mitamaStatus);
			checkUnchanged("FinalStatus", context.CapturedFinalStatus, finalStatus);
		}

		private void getDetails(GuiSession session, out string mitamaStatus, out string finalStatus)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			AutomationElementMap elementMap = getResultViewElementMap(session);

			AutomationElement mitamaStatusElement = elementMap.Get(AutomationIds.ResultViewForm.MITAMA_STATUS);

			mitamaStatus = mTextBoxOperator.GetText(mitamaStatusElement);

			if (string.IsNullOrWhiteSpace(mitamaStatus))
			{
				throw new InvalidOperationException("Calculation detail for Mitama-only status is empty.");
			}

			AutomationElement finalStatusElement = elementMap.Get(AutomationIds.ResultViewForm.FINAL_STATUS);

			finalStatus = mTextBoxOperator.GetText(finalStatusElement);

			if (string.IsNullOrWhiteSpace(finalStatus))
			{
				throw new InvalidOperationException("Calculation detail for final status is empty.");
			}
		}

		private static void checkUnchanged(string target, string expected, string actual)
		{
			if (string.Equals(expected, actual, StringComparison.Ordinal))
			{
				return;
			}

			int index = 0;
			int length = Math.Min(expected.Length, actual.Length);

			while (index < length && expected[index] == actual[index])
			{
				index++;
			}

			int start = Math.Max(0, index - 16);
			int expectedCount = Math.Min(expected.Length - start, 48);
			int actualCount = Math.Min(actual.Length - start, 48);

			string expectedPart = expected.Substring(start, expectedCount).Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
			string actualPart = actual.Substring(start, actualCount).Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");

			throw new InvalidOperationException($"Calculation detail has changed: {target}, FirstDifferenceIndex={index}, " + $"ExpectedLength={expected.Length}, ActualLength={actual.Length}, Expected=\"{expectedPart}\", Actual=\"{actualPart}\"");
		}

		public void Close(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			string stage = "GetResultViewForm";

			try
			{
				Window resultViewForm = getResultViewForm(session);

				AutomationElementMap elementMap = getResultViewElementMap(session);

				stage = "CloseResultView";

				AutomationElement closeButton = elementMap.Get(AutomationIds.ResultViewForm.CLOSE, ControlType.Button);

				mButtonOperator.PostClick(closeButton);

				stage = "WaitForWindowClosed";

				mWindowWaiter.WaitForWindowClosed(resultViewForm);

				resetResultViewCache();
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"CalculationDetailOperator.Close failed at stage '{stage}': " + $"{ex.Message}", ex);
			}
		}

		private Window getResultViewForm(GuiSession session)
		{
			if (ReferenceEquals(mResultViewSession, session) && mResultViewForm != null)
			{
				return mResultViewForm;
			}

			Window mainWindow = mGuiOperator.GetMainWindow(session);

			mResultViewForm = mProcessWindowWaiter.WaitForProcessWindow(session, mainWindow, element => element.Properties.AutomationId.ValueOrDefault == AutomationIds.ResultViewForm.ID);
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
