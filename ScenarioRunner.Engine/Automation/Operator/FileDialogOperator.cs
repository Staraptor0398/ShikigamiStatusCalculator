using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ScenarioRunner.Automation.Operator
{
	public class FileDialogOperator
	{
		private const uint WM_SETTEXT = 0x000C;
		private const uint WM_GETTEXT = 0x000D;
		private const uint WM_GETTEXTLENGTH = 0x000E;
		private const uint BM_CLICK = 0x00F5;

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, string lParam);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, StringBuilder lParam);

		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		private static extern int GetDlgCtrlID(IntPtr hWnd);

		public string LastSelectionInfo { get; private set; }

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

			LastSelectionInfo = null;

			AutomationElement[] fileNameComboBoxCandidates = getFileNameComboBoxCandidates(dialog);
			AutomationElement fileNameComboBox = selectFileNameComboBox(fileNameComboBoxCandidates);
			AutomationElement fileNameEdit = getFileNameEdit(fileNameComboBox);
			AutomationElement openButton = getOpenButton(dialog);

			IntPtr fileNameEditHandle = getNativeWindowHandle(fileNameEdit, "File name input");
			IntPtr openButtonHandle = getNativeWindowHandle(openButton, "Open button");

			setFilePath(fileNameEditHandle, filePath);

			string actualFilePath = getText(fileNameEditHandle);

			LastSelectionInfo = createSelectionInfo(
				dialog,
				fileNameComboBoxCandidates,
				fileNameComboBox,
				fileNameEdit,
				openButton,
				filePath,
				actualFilePath);

			SendMessage(openButtonHandle, BM_CLICK, IntPtr.Zero, IntPtr.Zero);
		}

		private AutomationElement[] getFileNameComboBoxCandidates(Window dialog)
		{
			AutomationElement[] comboBoxes = dialog.FindAllDescendants(cf => cf.ByControlType(ControlType.ComboBox));

			AutomationElement[] candidates = comboBoxes
				.Where(comboBox => comboBox.FindFirstDescendant(cf => cf.ByControlType(ControlType.Edit)) != null)
				.ToArray();

			if (candidates.Length == 0)
			{
				throw new InvalidOperationException("File name ComboBox was not found.");
			}

			return candidates;
		}

		private AutomationElement selectFileNameComboBox(AutomationElement[] candidates)
		{
			AutomationElement[] nativeCandidates = candidates.Where(isNativeComboBox).ToArray();

			if (nativeCandidates.Length > 0)
			{
				return nativeCandidates.OrderByDescending(comboBox => comboBox.BoundingRectangle.Y).First();
			}

			/*
			 * 標準Win32 ComboBoxとして識別できない環境では、
			 * 従来どおり最も下に配置されている編集可能ComboBoxを優先する。
			 */
			return candidates.OrderByDescending(comboBox => comboBox.BoundingRectangle.Y).First();
		}

		private bool isNativeComboBox(AutomationElement comboBox)
		{
			string comboBoxClassName = comboBox.Properties.ClassName.ValueOrDefault;

			if (!string.Equals(comboBoxClassName, "ComboBox", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			AutomationElement edit = comboBox.FindFirstDescendant(cf => cf.ByControlType(ControlType.Edit));

			if (edit == null)
			{
				return false;
			}

			string editClassName = edit.Properties.ClassName.ValueOrDefault;

			return string.Equals(editClassName, "Edit", StringComparison.OrdinalIgnoreCase);
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

		private string getText(IntPtr handle)
		{
			int length = SendMessage(handle, WM_GETTEXTLENGTH, IntPtr.Zero, IntPtr.Zero).ToInt32();

			var builder = new StringBuilder(length + 1);

			SendMessage(handle, WM_GETTEXT, new IntPtr(builder.Capacity), builder);

			return builder.ToString();
		}

		private string createSelectionInfo(Window dialog, AutomationElement[] comboBoxCandidates, AutomationElement selectedComboBox, AutomationElement selectedEdit, AutomationElement openButton, string expectedFilePath, string actualFilePath)
		{
			var builder = new StringBuilder();

			builder.AppendLine();
			builder.AppendLine("FileDialog diagnostics:");

			appendElementInfo(builder, "Dialog", dialog);

			builder.AppendLine($"ComboBoxCandidates={comboBoxCandidates.Length}");

			for (int i = 0; i < comboBoxCandidates.Length; i++)
			{
				AutomationElement comboBox = comboBoxCandidates[i];
				AutomationElement edit = comboBox.FindFirstDescendant(cf => cf.ByControlType(ControlType.Edit));

				appendElementInfo(builder, $"ComboBox[{i}]", comboBox);

				if (edit != null)
				{
					appendElementInfo(builder, $"ComboBox[{i}].Edit", edit);
				}
			}

			appendElementInfo(builder, "SelectedComboBox", selectedComboBox);
			appendElementInfo(builder, "SelectedEdit", selectedEdit);
			appendElementInfo(builder, "OpenButton", openButton);

			builder.AppendLine($"ExpectedFilePath={expectedFilePath}");
			builder.AppendLine($"EditTextAfterWM_SETTEXT={actualFilePath}");

			return builder.ToString().TrimEnd();
		}

		private void appendElementInfo(StringBuilder builder, string label, AutomationElement element)
		{
			IntPtr handle = element.Properties.NativeWindowHandle.ValueOrDefault;
			int controlId = handle == IntPtr.Zero ? 0 : GetDlgCtrlID(handle);

			builder.AppendLine(
				$"{label}: " +
				$"Name={element.Properties.Name.ValueOrDefault}, " +
				$"AutomationId={element.Properties.AutomationId.ValueOrDefault}, " +
				$"ClassName={element.Properties.ClassName.ValueOrDefault}, " +
				$"ControlType={element.Properties.ControlType.ValueOrDefault}, " +
				$"NativeWindowHandle={handle}, " +
				$"ControlId={controlId}, " +
				$"Bounds={element.BoundingRectangle}");
		}
	}
}
