using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using System;
using System.Linq;

namespace ScenarioRunner.Automation.Operator
{
	public class FileDialogOperator
	{
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

			if (string.IsNullOrWhiteSpace(filePath))
			{
				throw new ArgumentException("File path is empty.", nameof(filePath));
			}

			AutomationElement fileNameComboBox = getFileNameComboBox(dialog);
			AutomationElement openButton = getOpenButton(dialog);

			mComboBoxOperator.SetValue(dialog, fileNameComboBox.AutomationId, filePath);
			mButtonOperator.Click(dialog, openButton.AutomationId);
		}

		private AutomationElement getFileNameComboBox(Window dialog)
		{
			AutomationElement[] comboBoxes = dialog.FindAllDescendants(cf => cf.ByControlType(ControlType.ComboBox));

			AutomationElement[] candidates = comboBoxes.Where(comboBox => comboBox.Patterns.Value.IsSupported && !comboBox.Patterns.Value.Pattern.IsReadOnly).ToArray();

			if (candidates.Length == 0)
			{
				throw new InvalidOperationException("File name ComboBox was not found.");
			}

			/*
			 * ファイル名入力欄は通常ダイアログ下部に配置される。
			 * 書き込み可能なComboBoxが複数存在する場合は、
			 * 最も下に配置されているものを優先する。
			 */
			return candidates.OrderByDescending(comboBox => comboBox.BoundingRectangle.Y).First();
		}

		private AutomationElement getOpenButton(Window dialog)
		{
			AutomationElement[] buttons = dialog.FindAllDescendants(cf => cf.ByControlType(ControlType.Button));

			AutomationElement openButton = buttons.FirstOrDefault(button => button.Name.StartsWith("開く", StringComparison.OrdinalIgnoreCase) || button.Name.StartsWith("Open", StringComparison.OrdinalIgnoreCase));

			if (openButton == null)
			{
				throw new InvalidOperationException("Open button was not found.");
			}

			return openButton;
		}
	}
}
