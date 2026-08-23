using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using System;

namespace ScenarioRunner.Automation.Operator
{
	public class DialogOperator
	{
		private const string STANDARD_DIALOG_CLASS_NAME = "#32770";

		public Window GetActiveDialog(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			Window[] windows = session.Application.GetAllTopLevelWindows(session.Automation);

			foreach (Window window in windows)
			{
				if (window.ClassName == STANDARD_DIALOG_CLASS_NAME)
				{
					return window;
				}
			}

			return null;
		}

		public bool Exists(GuiSession session)
		{
			return GetActiveDialog(session) != null;
		}

		public string GetMessage(GuiSession session)
		{
			Window dialog = GetActiveDialog(session);

			if (dialog == null)
			{
				return null;
			}

			var textElements = dialog.FindAllDescendants(cf => cf.ByControlType(ControlType.Text));

			foreach (var element in textElements)
			{
				if (!string.IsNullOrWhiteSpace(element.Name))
				{
					return element.Name;
				}
			}

			return null;
		}

		public void CheckMessage(GuiSession session, string expectedMessage)
		{
			if (string.IsNullOrWhiteSpace(expectedMessage))
			{
				throw new ArgumentException("Expected dialog message is empty.", nameof(expectedMessage));
			}

			Window dialog = GetActiveDialog(session);

			if (dialog == null)
			{
				throw new InvalidOperationException("Modal dialog was not found.");
			}

			var textElements = dialog.FindAllDescendants(cf => cf.ByControlType(ControlType.Text));

			foreach (var element in textElements)
			{
				string text = element.Name;

				if (!string.IsNullOrWhiteSpace(text) && text.Contains(expectedMessage))
				{
					return;
				}
			}

			throw new InvalidOperationException($"Expected dialog message was not found: {expectedMessage}");
		}

		public void Close(GuiSession session)
		{
			Window dialog = GetActiveDialog(session);

			if (dialog == null)
			{
				throw new InvalidOperationException("Modal dialog was not found.");
			}

			dialog.Close();
		}
	}
}
