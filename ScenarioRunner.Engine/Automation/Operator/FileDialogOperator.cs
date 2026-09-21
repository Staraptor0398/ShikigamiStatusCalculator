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

		public void SelectLoadFile(Window dialog, string filePath)
		{
			validateArguments(dialog, filePath);

			LastSelectionInfo = null;

			AutomationElement[] fileNameComboBoxCandidates = getFileNameComboBoxCandidates(dialog);
			AutomationElement fileNameComboBox = selectFileNameComboBox(fileNameComboBoxCandidates);
			AutomationElement fileNameEdit = getFileNameEdit(fileNameComboBox);
			AutomationElement openButton = getOpenButton(dialog);

			IntPtr fileNameEditHandle = getNativeWindowHandle(fileNameEdit, "File name input");
			IntPtr openButtonHandle = getNativeWindowHandle(openButton, "Open button");

			setFilePath(fileNameEditHandle, filePath);

			string actualFilePath = getText(fileNameEditHandle);

			LastSelectionInfo = createSelectionInfo(dialog, fileNameComboBoxCandidates, fileNameComboBox, fileNameEdit, openButton, filePath, actualFilePath);

			SendMessage(openButtonHandle, BM_CLICK, IntPtr.Zero, IntPtr.Zero);
		}

		public void SelectSaveFile(Window dialog, string filePath)
		{
			validateArguments(dialog, filePath);

			LastSelectionInfo = null;

			AutomationElement[] fileNameComboBoxCandidates = getFileNameComboBoxCandidates(dialog);
			AutomationElement fileNameComboBox = selectFileNameComboBox(fileNameComboBoxCandidates);
			AutomationElement fileNameEdit = getFileNameEdit(fileNameComboBox);
			AutomationElement saveButton = getSaveButton(dialog);

			IntPtr fileNameEditHandle = getNativeWindowHandle(fileNameEdit, "File name input");

			setFilePath(fileNameEditHandle, filePath);
			notifyFileNameChanged(fileNameEditHandle);

			string actualFilePath = getText(fileNameEditHandle);

			LastSelectionInfo = createSelectionInfo(dialog, fileNameComboBoxCandidates, fileNameComboBox, fileNameEdit, saveButton, filePath, actualFilePath);

			saveButton.AsButton().Invoke();
		}

		private void validateArguments(Window dialog, string filePath)
		{
			if (dialog == null)
			{
				throw new ArgumentNullException(nameof(dialog));
			}

			if (string.IsNullOrWhiteSpace(filePath))
			{
				throw new ArgumentException("File path is empty.", nameof(filePath));
			}
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
			AutomationElement fileNameComboBox = candidates.FirstOrDefault(comboBox => string.Equals(comboBox.Properties.AutomationId.ValueOrDefault, FILE_NAME_CONTROL_HOST, StringComparison.Ordinal));

			if (fileNameComboBox != null)
			{
				return fileNameComboBox;
			}

			AutomationElement[] nativeCandidates = candidates.Where(isNativeComboBox).ToArray();

			if (nativeCandidates.Length > 0)
			{
				return nativeCandidates.First();
			}

			return candidates.First();
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
			return getActionButton(dialog, "開く", "Open");
		}

		private AutomationElement getSaveButton(Window dialog)
		{
			return getActionButton(dialog, "保存", "Save");
		}

		private AutomationElement getActionButton(Window dialog, params string[] names)
		{
			AutomationElement[] buttons = dialog.FindAllDescendants(cf => cf.ByControlType(ControlType.Button));

			AutomationElement actionButton = buttons.FirstOrDefault(button => names.Any(name => button.Name.StartsWith(name, StringComparison.OrdinalIgnoreCase)));

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

		private string createSelectionInfo(Window dialog, AutomationElement[] comboBoxCandidates, AutomationElement selectedComboBox, AutomationElement selectedEdit, AutomationElement actionButton, string expectedFilePath, string actualFilePath)
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
