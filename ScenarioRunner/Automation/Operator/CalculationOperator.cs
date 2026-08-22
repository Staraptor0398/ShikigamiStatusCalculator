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

		public CalculationOperator()
		{
			mButtonOperator = new ButtonOperator();
			mDialogOperator = new DialogOperator();
			mTextBoxOperator = new TextBoxOperator();
			mComboBoxOperator = new ComboBoxOperator();
		}

		public void Calculate(GuiSession session)
		{
			mButtonOperator.Click(session.MainWindow, CALCULATE_BUTTON_AUTOMATION_ID);
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

			string mitamaOnly = mTextBoxOperator.GetText(session.MainWindow, MITAMA_ONLY_TEXT_BOX_AUTOMATION_ID);

			if (string.IsNullOrWhiteSpace(mitamaOnly))
			{
				throw new InvalidOperationException("Calculation result for Mitama-only status is empty.");
			}

			string shikigami = mComboBoxOperator.GetValue(session.MainWindow, SHIKIGAMI_COMBO_BOX_AUTOMATION_ID);

			if (!string.IsNullOrWhiteSpace(shikigami))
			{
				string finalStats = mTextBoxOperator.GetText(session.MainWindow, FINAL_STATS_TEXT_BOX_AUTOMATION_ID);

				if (string.IsNullOrWhiteSpace(finalStats))
				{
					throw new InvalidOperationException("Calculation result for final status is empty.");
				}
			}
		}
	}
}
