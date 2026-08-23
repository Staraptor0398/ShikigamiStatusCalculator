using FlaUI.Core.AutomationElements;
using System;

namespace ScenarioRunner.Automation.Operator
{
	public class CalculationOperator
	{
		private const string CALCULATE_BUTTON_AUTOMATION_ID = "btnCalc";

		private const string MITAMA_ONLY_TEXT_BOX_AUTOMATION_ID = "txtMitamaOnly";
		private const string FINAL_STATS_TEXT_BOX_AUTOMATION_ID = "txtFinalStats";
		private const string SHIKIGAMI_COMBO_BOX_AUTOMATION_ID = "cmbShikigami";

		private readonly ButtonOperator mButtonOperator;
		private readonly DialogOperator mDialogOperator;
		private readonly TextBoxOperator mTextBoxOperator;
		private readonly ComboBoxOperator mComboBoxOperator;
		private readonly GuiOperator mGuiOperator;

		public CalculationOperator()
		{
			mButtonOperator = new ButtonOperator();
			mDialogOperator = new DialogOperator();
			mTextBoxOperator = new TextBoxOperator();
			mComboBoxOperator = new ComboBoxOperator();
			mGuiOperator = new GuiOperator();
		}

		public void Calculate(GuiSession session)
		{
			Window mainWindow = mGuiOperator.GetMainWindow(session);
			mButtonOperator.Click(mainWindow, CALCULATE_BUTTON_AUTOMATION_ID);
		}

		public void Check(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			if (mDialogOperator.Exists(session))
			{
				throw new InvalidOperationException("A modal dialog is displayed after calculation.");
			}

			Window mainWindow = mGuiOperator.GetMainWindow(session);

			string mitamaOnly = mTextBoxOperator.GetText(mainWindow, MITAMA_ONLY_TEXT_BOX_AUTOMATION_ID);

			if (string.IsNullOrWhiteSpace(mitamaOnly))
			{
				throw new InvalidOperationException("Calculation result for Mitama-only status is empty.");
			}

			string shikigami = mComboBoxOperator.GetValue(mainWindow, SHIKIGAMI_COMBO_BOX_AUTOMATION_ID);

			if (!string.IsNullOrWhiteSpace(shikigami))
			{
				string finalStats = mTextBoxOperator.GetText(mainWindow, FINAL_STATS_TEXT_BOX_AUTOMATION_ID);

				if (string.IsNullOrWhiteSpace(finalStats))
				{
					throw new InvalidOperationException("Calculation result for final status is empty.");
				}
			}
		}
	}
}
