using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using System;
using System.Runtime.InteropServices;

namespace ScenarioRunner.Automation.Operator
{
	public class ComboBoxOperator
	{
		private const uint CB_GETCOUNT = 0x0146;
		private const uint CB_SETCURSEL = 0x014E;
		private const uint CB_FINDSTRINGEXACT = 0x0158;
		private const uint WM_COMMAND = 0x0111;

		private const int CBN_SELCHANGE = 1;
		private const int CB_ERR = -1;

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, string lParam);

		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		private static extern IntPtr GetParent(IntPtr hWnd);

		[DllImport("user32.dll")]
		private static extern int GetDlgCtrlID(IntPtr hWnd);

		public void SelectItem(AutomationElement parent, string automationId, string itemName)
		{
			ComboBox comboBox = getComboBox(parent, automationId);
			IntPtr handle = comboBox.Properties.NativeWindowHandle.Value;

			int index = SendMessage(handle, CB_FINDSTRINGEXACT, new IntPtr(-1), itemName).ToInt32();

			if (index == CB_ERR)
			{
				throw new InvalidOperationException($"ComboBox item was not found: {itemName}");
			}

			selectItem(handle, index);
		}

		public void SetValue(AutomationElement parent, string automationId, string value)
		{
			ComboBox comboBox = getComboBox(parent, automationId);

			var editElement = comboBox.FindFirstDescendant(cf => cf.ByControlType(ControlType.Edit));

			if (editElement == null)
			{
				throw new InvalidOperationException($"Editable area of ComboBox was not found: {automationId}");
			}

			if (!editElement.Patterns.Value.IsSupported)
			{
				throw new InvalidOperationException($"ValuePattern is not supported by editable area of ComboBox: {automationId}");
			}

			editElement.Patterns.Value.Pattern.SetValue(value);
		}

		public string GetValue(AutomationElement parent, string automationId)
		{
			ComboBox comboBox = getComboBox(parent, automationId);

			return comboBox.Value;
		}

		public bool CanSelectFirstItem(AutomationElement parent, string automationId)
		{
			ComboBox comboBox = getComboBox(parent, automationId);
			IntPtr handle = comboBox.Properties.NativeWindowHandle.Value;

			int itemCount = SendMessage(handle, CB_GETCOUNT, IntPtr.Zero, IntPtr.Zero).ToInt32();

			if (itemCount <= 0)
			{
				return false;
			}

			selectItem(handle, 0);

			return !string.IsNullOrWhiteSpace(comboBox.Value);
		}

		public void SelectFirstItem(AutomationElement parent, string automationId)
		{
			ComboBox comboBox = getComboBox(parent, automationId);
			IntPtr handle = comboBox.Properties.NativeWindowHandle.Value;

			int itemCount = SendMessage(handle, CB_GETCOUNT, IntPtr.Zero, IntPtr.Zero).ToInt32();

			if (itemCount <= 0)
			{
				throw new InvalidOperationException($"ComboBox has no items: {automationId}");
			}

			selectItem(handle, 0);
		}

		private void selectItem(IntPtr handle, int index)
		{
			int result = SendMessage(handle, CB_SETCURSEL, new IntPtr(index), IntPtr.Zero).ToInt32();

			if (result == CB_ERR)
			{
				throw new InvalidOperationException($"ComboBox item could not be selected: index={index}");
			}

			IntPtr parentHandle = GetParent(handle);
			int controlId = GetDlgCtrlID(handle);

			IntPtr wParam = new IntPtr((controlId & 0xFFFF) | (CBN_SELCHANGE << 16));

			SendMessage(parentHandle, WM_COMMAND, wParam, handle);
		}

		private ComboBox getComboBox(AutomationElement parent, string automationId)
		{
			if (parent == null)
			{
				throw new ArgumentNullException(nameof(parent));
			}

			var comboBoxElement = parent.FindFirstDescendant(cf => cf.ByAutomationId(automationId).And(cf.ByControlType(ControlType.ComboBox)));

			if (comboBoxElement == null)
			{
				throw new InvalidOperationException($"ComboBox was not found: {automationId}");
			}

			return comboBoxElement.AsComboBox();
		}
	}
}
