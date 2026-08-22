using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using System;
using System.Runtime.InteropServices;

namespace ScenarioRunner.Automation.Operator
{
	public class ComboBoxOperator
	{
		private const uint CB_FINDSTRINGEXACT = 0x0158;
		private const int CB_ERR = -1;

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, string lParam);

		public void SelectItem(AutomationElement parent, string automationId, string itemName)
		{
			ComboBox comboBox = getComboBox(parent, automationId);

			IntPtr handle = comboBox.Properties.NativeWindowHandle.Value;

			int index = SendMessage(handle, CB_FINDSTRINGEXACT, new IntPtr(-1), itemName).ToInt32();

			if (index == CB_ERR)
			{
				throw new InvalidOperationException($"ComboBox item was not found: {itemName}");
			}

			comboBox.Focus();

			Keyboard.Type(VirtualKeyShort.HOME);

			for (int i = 0; i < index; i++)
			{
				Keyboard.Type(VirtualKeyShort.DOWN);
			}

			Wait.UntilInputIsProcessed();
		}

		public void SetValue(AutomationElement parent, string automationId, string value)
		{
			ComboBox comboBox = getComboBox(parent, automationId);

			var editElement = comboBox.FindFirstDescendant(
				cf => cf.ByControlType(ControlType.Edit));

			if (editElement == null)
			{
				throw new InvalidOperationException($"Editable area of ComboBox was not found: {automationId}");
			}

			var edit = editElement.AsTextBox();

			edit.Focus();

			Keyboard.Press(VirtualKeyShort.CONTROL);
			Keyboard.Type(VirtualKeyShort.KEY_A);
			Keyboard.Release(VirtualKeyShort.CONTROL);

			Keyboard.Type(value);

			Wait.UntilInputIsProcessed();
		}

		public string GetValue(AutomationElement parent, string automationId)
		{
			ComboBox comboBox = getComboBox(parent, automationId);

			return comboBox.Value;
		}

		private ComboBox getComboBox(AutomationElement parent, string automationId)
		{
			if (parent == null)
			{
				throw new ArgumentNullException(nameof(parent));
			}

			var comboBoxElement = parent.FindFirstDescendant(
				cf => cf
					.ByAutomationId(automationId)
					.And(cf.ByControlType(ControlType.ComboBox)));

			if (comboBoxElement == null)
			{
				throw new InvalidOperationException($"ComboBox was not found: {automationId}");
			}

			return comboBoxElement.AsComboBox();
		}
	}
}
