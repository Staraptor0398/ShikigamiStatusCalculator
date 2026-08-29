using FlaUI.Core.AutomationElements;
using ScenarioRunner.Automation.Definition;
using System;

namespace ScenarioRunner.Automation.Operator
{
	public class CalculationOperator
	{
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
			mButtonOperator.Click(mainWindow, AutomationIds.MainForm.CALCULATE);
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

			string mitamaOnly = mTextBoxOperator.GetText(mainWindow, AutomationIds.MainForm.MITAMA_ONLY);

			if (string.IsNullOrWhiteSpace(mitamaOnly))
			{
				throw new InvalidOperationException("Calculation result for Mitama-only status is empty.");
			}

			string shikigami = mComboBoxOperator.GetValue(mainWindow, AutomationIds.MainForm.SHIKIGAMI);

			if (!string.IsNullOrWhiteSpace(shikigami))
			{
				string finalStats = mTextBoxOperator.GetText(mainWindow, AutomationIds.MainForm.FINAL_STATS);

				if (string.IsNullOrWhiteSpace(finalStats))
				{
					throw new InvalidOperationException("Calculation result for final status is empty.");
				}
			}
		}
	}
}
