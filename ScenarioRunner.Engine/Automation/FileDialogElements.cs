using FlaUI.Core.AutomationElements;
using System;

namespace ScenarioRunner.Automation
{
	public class FileDialogElements
	{
		public Window Dialog { get; }
		public AutomationElement[] Descendants { get; }

		public FileDialogElements(Window dialog, AutomationElement[] descendants)
		{
			if (dialog == null)
			{
				throw new ArgumentNullException(nameof(dialog));
			}

			if (descendants == null)
			{
				throw new ArgumentNullException(nameof(descendants));
			}

			Dialog = dialog;
			Descendants = descendants;
		}
	}
}
