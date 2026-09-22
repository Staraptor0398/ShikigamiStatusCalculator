using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ScenarioRunner.Automation.Operator
{
	public class ComboBoxOperator
	{
		private const uint CB_GETCOUNT = 0x0146;
		private const uint CB_GETLBTEXT = 0x0148;
		private const uint CB_GETLBTEXTLEN = 0x0149;
		private const uint CB_SETCURSEL = 0x014E;
		private const uint CB_FINDSTRINGEXACT = 0x0158;
		private const uint WM_COMMAND = 0x0111;

		private const int CBN_SELCHANGE = 1;
		private const int CB_ERR = -1;

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, string lParam);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, StringBuilder lParam);

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

		public void SelectItem(AutomationElement element, string itemName)
		{
			if (element == null)
			{
				throw new ArgumentNullException(nameof(element));
			}

			ComboBox comboBox = element.AsComboBox();
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

		public string GetValue(AutomationElement element)
		{
			if (element == null)
			{
				throw new ArgumentNullException(nameof(element));
			}

			return element.AsComboBox().Value;
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

		public IReadOnlyList<string> GetItems(AutomationElement parent, string automationId)
		{
			ComboBox comboBox = getComboBox(parent, automationId);
			IntPtr handle = comboBox.Properties.NativeWindowHandle.Value;

			int itemCount = SendMessage(handle, CB_GETCOUNT, IntPtr.Zero, IntPtr.Zero).ToInt32();

			if (itemCount == CB_ERR)
			{
				throw new InvalidOperationException($"ComboBox item count could not be obtained: {automationId}");
			}

			var items = new List<string>();

			for (int i = 0; i < itemCount; i++)
			{
				int textLength = SendMessage(handle, CB_GETLBTEXTLEN, new IntPtr(i), IntPtr.Zero).ToInt32();

				if (textLength == CB_ERR)
				{
					throw new InvalidOperationException($"ComboBox item text length could not be obtained: {automationId}, index={i}");
				}

				var text = new StringBuilder(textLength + 1);

				int result = SendMessage(handle, CB_GETLBTEXT, new IntPtr(i), text).ToInt32();

				if (result == CB_ERR)
				{
					throw new InvalidOperationException($"ComboBox item text could not be obtained: {automationId}, index={i}");
				}

				items.Add(text.ToString());
			}

			return items;
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
