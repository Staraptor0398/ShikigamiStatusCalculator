using FlaUI.Core.AutomationElements;
using System;

namespace ScenarioRunner.Automation
{
	public class FileDialogElements
	{
		public Window Dialog { get; }
		public AutomationElement[] Descendants { get; }
		public AutomationElement[] FileNameComboBoxCandidates { get; }
		public AutomationElement[] FileNameEdits { get; }

		public FileDialogElements(Window dialog, AutomationElement[] descendants, AutomationElement[] fileNameComboBoxCandidates, AutomationElement[] fileNameEdits)
		{
			if (dialog == null)
			{
				throw new ArgumentNullException(nameof(dialog));
			}

			if (descendants == null)
			{
				throw new ArgumentNullException(nameof(descendants));
			}

			if (fileNameComboBoxCandidates == null)
			{
				throw new ArgumentNullException(nameof(fileNameComboBoxCandidates));
			}

			if (fileNameEdits == null)
			{
				throw new ArgumentNullException(nameof(fileNameEdits));
			}

			if (fileNameComboBoxCandidates.Length != fileNameEdits.Length)
			{
				throw new ArgumentException("File name ComboBox and Edit counts do not match.");
			}

			Dialog = dialog;
			Descendants = descendants;
			FileNameComboBoxCandidates = fileNameComboBoxCandidates;
			FileNameEdits = fileNameEdits;
		}
	}
}
