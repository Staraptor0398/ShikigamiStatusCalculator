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

			Window[] windows = session.Application.GetAllTopLevelWindows(session.Automation);

			foreach (Window window in windows)
			{
				if (window.Equals(session.MainWindow))
				{
					continue;
				}

				if (window.ControlType != ControlType.Window)
				{
					continue;
				}

				if (!window.IsModal)
				{
					continue;
				}

				return window;
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

			string message = string.Join(Environment.NewLine, textElements.Select(element => element.Name).Where(text => !string.IsNullOrWhiteSpace(text)));

			return message;
		}

		public bool HasMessage(GuiSession session, string expectedMessage)
		{
			string message = GetMessage(session);

			if (string.IsNullOrEmpty(message))
			{
				return false;
			}

			return message.Contains(expectedMessage);
		}
	}
}
