using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using ScenarioRunner.Automation.Definition;
using System;

namespace ScenarioRunner.Automation.Operator.Feature
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
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			AutomationElement calculateButton = mGuiOperator.GetMainElement(session, AutomationIds.MainForm.CALCULATE, ControlType.Button);

			mButtonOperator.Click(calculateButton);
		}

		public void Check(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			AutomationElement mitamaOnlyElement = mGuiOperator.GetMainElement(session, AutomationIds.MainForm.MITAMA_ONLY);

			string mitamaOnly = mTextBoxOperator.GetText(mitamaOnlyElement);

			if (string.IsNullOrWhiteSpace(mitamaOnly))
			{
				throw new InvalidOperationException("Calculation result for Mitama-only status is empty.");
			}

			AutomationElement shikigamiElement = mGuiOperator.GetMainElement(session, AutomationIds.MainForm.SHIKIGAMI, ControlType.ComboBox);

			string shikigami = mComboBoxOperator.GetValue(shikigamiElement);

			if (!string.IsNullOrWhiteSpace(shikigami))
			{
				AutomationElement finalStatsElement = mGuiOperator.GetMainElement(session, AutomationIds.MainForm.FINAL_STATS);

				string finalStats = mTextBoxOperator.GetText(finalStatsElement);

				if (string.IsNullOrWhiteSpace(finalStats))
				{
					throw new InvalidOperationException("Calculation result for final status is empty.");
				}
			}
		}
	}
}
