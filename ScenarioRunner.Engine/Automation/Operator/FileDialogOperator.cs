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
		private const uint WM_COMMAND = 0x0111;
		private const uint BM_CLICK = 0x00F5;

		private const int EN_CHANGE = 0x0300;

		private const string FILE_NAME_CONTROL_HOST = "FileNameControlHost";

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, string lParam);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, StringBuilder lParam);

		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		private static extern int GetDlgCtrlID(IntPtr hWnd);

		[DllImport("user32.dll")]
		private static extern IntPtr GetParent(IntPtr hWnd);

		public string LastSelectionInfo { get; private set; }

		public void SelectLoadFile(FileDialogElements fileDialogElements, string filePath)
		{
			validateArguments(fileDialogElements, filePath);

			LastSelectionInfo = null;

			Window dialog = fileDialogElements.Dialog;
			AutomationElement[] descendants = fileDialogElements.Descendants;
			AutomationElement[] fileNameComboBoxCandidates = fileDialogElements.FileNameComboBoxCandidates;
			AutomationElement[] fileNameEdits = fileDialogElements.FileNameEdits;

			int selectedIndex = selectFileNameComboBoxIndex(fileNameComboBoxCandidates, fileNameEdits);

			AutomationElement fileNameComboBox = fileNameComboBoxCandidates[selectedIndex];
			AutomationElement fileNameEdit = fileNameEdits[selectedIndex];
			AutomationElement openButton = getOpenButton(descendants);

			IntPtr fileNameEditHandle = getNativeWindowHandle(fileNameEdit, "File name input");
			IntPtr openButtonHandle = getNativeWindowHandle(openButton, "Open button");

			setFilePath(fileNameEditHandle, filePath);

			string actualFilePath = getText(fileNameEditHandle);

			LastSelectionInfo = createSelectionInfo(dialog, fileNameComboBoxCandidates, fileNameEdits, fileNameComboBox, fileNameEdit, openButton, filePath, actualFilePath);

			SendMessage(openButtonHandle, BM_CLICK, IntPtr.Zero, IntPtr.Zero);
		}

		public void SelectSaveFile(FileDialogElements fileDialogElements, string filePath)
		{
			validateArguments(fileDialogElements, filePath);

			LastSelectionInfo = null;

			Window dialog = fileDialogElements.Dialog;
			AutomationElement[] descendants = fileDialogElements.Descendants;
			AutomationElement[] fileNameComboBoxCandidates = fileDialogElements.FileNameComboBoxCandidates;
			AutomationElement[] fileNameEdits = fileDialogElements.FileNameEdits;

			int selectedIndex = selectFileNameComboBoxIndex(fileNameComboBoxCandidates, fileNameEdits);

			AutomationElement fileNameComboBox = fileNameComboBoxCandidates[selectedIndex];
			AutomationElement fileNameEdit = fileNameEdits[selectedIndex];
			AutomationElement saveButton = getSaveButton(descendants);

			IntPtr fileNameEditHandle = getNativeWindowHandle(fileNameEdit, "File name input");

			setFilePath(fileNameEditHandle, filePath);
			notifyFileNameChanged(fileNameEditHandle);

			string actualFilePath = getText(fileNameEditHandle);

			LastSelectionInfo = createSelectionInfo(dialog, fileNameComboBoxCandidates, fileNameEdits, fileNameComboBox, fileNameEdit, saveButton, filePath, actualFilePath);

			saveButton.AsButton().Invoke();
		}

		private void validateArguments(FileDialogElements fileDialogElements, string filePath)
		{
			if (fileDialogElements == null)
			{
				throw new ArgumentNullException(nameof(fileDialogElements));
			}

			if (string.IsNullOrWhiteSpace(filePath))
			{
				throw new ArgumentException("File path is empty.", nameof(filePath));
			}
		}

		private int selectFileNameComboBoxIndex(AutomationElement[] candidates, AutomationElement[] edits)
		{
			for (int i = 0; i < candidates.Length; i++)
			{
				if (string.Equals(candidates[i].Properties.AutomationId.ValueOrDefault, FILE_NAME_CONTROL_HOST, StringComparison.Ordinal))
				{
					return i;
				}
			}

			for (int i = 0; i < candidates.Length; i++)
			{
				if (isNativeComboBox(candidates[i], edits[i]))
				{
					return i;
				}
			}

			return 0;
		}

		private bool isNativeComboBox(AutomationElement comboBox, AutomationElement edit)
		{
			string comboBoxClassName = comboBox.Properties.ClassName.ValueOrDefault;

			if (!string.Equals(comboBoxClassName, "ComboBox", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			string editClassName = edit.Properties.ClassName.ValueOrDefault;

			return string.Equals(editClassName, "Edit", StringComparison.OrdinalIgnoreCase);
		}

		private AutomationElement getOpenButton(AutomationElement[] descendants)
		{
			return getActionButton(descendants, "開く", "Open");
		}

		private AutomationElement getSaveButton(AutomationElement[] descendants)
		{
			return getActionButton(descendants, "保存", "Save");
		}

		private AutomationElement getActionButton(AutomationElement[] descendants, params string[] names)
		{
			AutomationElement actionButton = descendants.FirstOrDefault(element => element.Properties.ControlType.ValueOrDefault == ControlType.Button && names.Any(name => element.Name.StartsWith(name, StringComparison.OrdinalIgnoreCase)));

			if (actionButton == null)
			{
				throw new InvalidOperationException($"File dialog action button was not found: {string.Join("/", names)}");
			}

			return actionButton;
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

		private void notifyFileNameChanged(IntPtr fileNameEditHandle)
		{
			IntPtr parentHandle = GetParent(fileNameEditHandle);

			if (parentHandle == IntPtr.Zero)
			{
				throw new InvalidOperationException("File name input parent window was not found.");
			}

			int controlId = GetDlgCtrlID(fileNameEditHandle);
			IntPtr wParam = makeWParam(controlId, EN_CHANGE);

			SendMessage(parentHandle, WM_COMMAND, wParam, fileNameEditHandle);
		}

		private IntPtr makeWParam(int lowWord, int highWord)
		{
			int value = (lowWord & 0xFFFF) | ((highWord & 0xFFFF) << 16);

			return new IntPtr(value);
		}

		private string getText(IntPtr handle)
		{
			int length = SendMessage(handle, WM_GETTEXTLENGTH, IntPtr.Zero, IntPtr.Zero).ToInt32();

			var builder = new StringBuilder(length + 1);

			SendMessage(handle, WM_GETTEXT, new IntPtr(builder.Capacity), builder);

			return builder.ToString();
		}

		private string createSelectionInfo(Window dialog, AutomationElement[] comboBoxCandidates, AutomationElement[] editCandidates, AutomationElement selectedComboBox, AutomationElement selectedEdit, AutomationElement actionButton, string expectedFilePath, string actualFilePath)
		{
			var builder = new StringBuilder();

			builder.AppendLine();
			builder.AppendLine("FileDialog diagnostics:");

			appendElementInfo(builder, "Dialog", dialog);

			builder.AppendLine($"ComboBoxCandidates={comboBoxCandidates.Length}");

			for (int i = 0; i < comboBoxCandidates.Length; i++)
			{
				appendElementInfo(builder, $"ComboBox[{i}]", comboBoxCandidates[i]);
				appendElementInfo(builder, $"ComboBox[{i}].Edit", editCandidates[i]);
			}

			appendElementInfo(builder, "SelectedComboBox", selectedComboBox);
			appendElementInfo(builder, "SelectedEdit", selectedEdit);
			appendElementInfo(builder, "ActionButton", actionButton);

			builder.AppendLine($"ExpectedFilePath={expectedFilePath}");
			builder.AppendLine($"FileNameTextAfterSet={actualFilePath}");

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
