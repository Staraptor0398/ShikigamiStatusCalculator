using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace ScenarioRunner.Automation.Operator
{
	public class FileDialogOperator
	{
		private const uint WM_SETTEXT = 0x000C;
		private const uint BM_CLICK = 0x00F5;

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, string lParam);

		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

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
			AutomationElement fileNameEdit = getFileNameEdit(fileNameComboBox);
			AutomationElement openButton = getOpenButton(dialog);

			IntPtr fileNameEditHandle = getNativeWindowHandle(fileNameEdit, "File name input");
			IntPtr openButtonHandle = getNativeWindowHandle(openButton, "Open button");

			setFilePath(fileNameEditHandle, filePath);

			SendMessage(openButtonHandle, BM_CLICK, IntPtr.Zero, IntPtr.Zero);
		}

		private AutomationElement getFileNameComboBox(Window dialog)
		{
			AutomationElement[] comboBoxes = dialog.FindAllDescendants(cf => cf.ByControlType(ControlType.ComboBox));

			AutomationElement[] candidates = comboBoxes
				.Where(comboBox => comboBox.FindFirstDescendant(cf => cf.ByControlType(ControlType.Edit)) != null)
				.ToArray();

			if (candidates.Length == 0)
			{
				throw new InvalidOperationException("File name ComboBox was not found.");
			}

			/*
			 * ファイル名入力欄は通常ダイアログ下部に配置される。
			 * 編集可能なComboBoxが複数存在する場合は、
			 * 最も下に配置されているものを優先する。
			 */
			return candidates.OrderByDescending(comboBox => comboBox.BoundingRectangle.Y).First();
		}

		private AutomationElement getFileNameEdit(AutomationElement comboBox)
		{
			AutomationElement edit = comboBox.FindFirstDescendant(cf => cf.ByControlType(ControlType.Edit));

			if (edit == null)
			{
				throw new InvalidOperationException("File name input was not found.");
			}

			return edit;
		}

		private AutomationElement getOpenButton(Window dialog)
		{
			AutomationElement[] buttons = dialog.FindAllDescendants(cf => cf.ByControlType(ControlType.Button));

			AutomationElement openButton = buttons.FirstOrDefault(button =>
				button.Name.StartsWith("開く", StringComparison.OrdinalIgnoreCase) ||
				button.Name.StartsWith("Open", StringComparison.OrdinalIgnoreCase));

			if (openButton == null)
			{
				throw new InvalidOperationException("Open button was not found.");
			}

			return openButton;
		}

		private IntPtr getNativeWindowHandle(AutomationElement element, string elementName)
		{
			IntPtr handle = element.Properties.NativeWindowHandle.Value;

			if (handle == IntPtr.Zero)
			{
				throw new InvalidOperationException($"{elementName} has no native window handle.");
			}

			return handle;
		}

		private void setFilePath(IntPtr handle, string filePath)
		{
			IntPtr result = SendMessage(handle, WM_SETTEXT, IntPtr.Zero, filePath);

			if (result == IntPtr.Zero)
			{
				throw new InvalidOperationException("File path could not be set.");
			}
		}
	}
}
