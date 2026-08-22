using FlaUI.Core.AutomationElements;
using System;

namespace ScenarioRunner.Automation.Operator
{
	public class FileDialogOperator
	{
		private const string FILE_NAME_COMBO_BOX_AUTOMATION_ID = "1148";
		private const string OPEN_BUTTON_AUTOMATION_ID = "1";

		private readonly ComboBoxOperator mComboBoxOperator;
		private readonly ButtonOperator mButtonOperator;

		public FileDialogOperator()
		{
			mComboBoxOperator = new ComboBoxOperator();
			mButtonOperator = new ButtonOperator();
		}

		public void SelectFile(Window dialog, string filePath)
		{
			if (dialog == null)
			{
				throw new ArgumentNullException(nameof(dialog));
			}

			mComboBoxOperator.SetValue(dialog, FILE_NAME_COMBO_BOX_AUTOMATION_ID, filePath);
			mButtonOperator.Click(dialog, OPEN_BUTTON_AUTOMATION_ID);
		}
	}
}
