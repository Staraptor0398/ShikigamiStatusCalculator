using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using System;
using System.Linq;

namespace ScenarioRunner.Automation.Operator
{
	public class DialogOperator
	{
		public Window GetActiveDialog(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			Window[] modalWindows = session.MainWindow.ModalWindows;

			if (modalWindows == null || modalWindows.Length == 0)
			{
				return null;
			}

			return modalWindows[0];
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

			string message = string.Join(Environment.NewLine, textElements.Select(element => element.Name).Where(text => !string.IsNullOrWhiteSpace(text)));

			return message;
		}

		public void CheckMessage(GuiSession session, string expectedMessage)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

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
