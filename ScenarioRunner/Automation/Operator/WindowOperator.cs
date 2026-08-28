using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using System;
using System.Linq;
using System.Threading;

namespace ScenarioRunner.Automation.Operator
{
	public class WindowOperator
	{
		private const int DEFAULT_TIMEOUT_MS = 5000;
		private const int DEFAULT_INTERVAL_MS = 100;

		public Window FindWindow(GuiSession session, Func<AutomationElement, bool> predicate)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			if (predicate == null)
			{
				throw new ArgumentNullException(nameof(predicate));
			}

			AutomationElement desktop = session.Automation.GetDesktop();

			AutomationElement windowElement = desktop.FindAllChildren(cf => cf.ByControlType(ControlType.Window)).FirstOrDefault(predicate);

			return windowElement?.AsWindow();
		}

		public bool Exists(GuiSession session, Func<AutomationElement, bool> predicate)
		{
			return FindWindow(session, predicate) != null;
		}

		public Window WaitForWindow(GuiSession session, Func<AutomationElement, bool> predicate)
		{
			return WaitForWindow(session, predicate, DEFAULT_TIMEOUT_MS, DEFAULT_INTERVAL_MS);
		}

		public Window WaitForWindow(GuiSession session, Func<AutomationElement, bool> predicate, int timeoutMs, int intervalMs)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			if (predicate == null)
			{
				throw new ArgumentNullException(nameof(predicate));
			}

			if (timeoutMs <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(timeoutMs));
			}

			if (intervalMs <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(intervalMs));
			}

			int elapsed = 0;

			while (elapsed < timeoutMs)
			{
				Window window = FindWindow(session, predicate);

				if (window != null)
				{
					return window;
				}

				Thread.Sleep(intervalMs);
				elapsed += intervalMs;
			}

			throw new InvalidOperationException($"Window was not found within {timeoutMs} ms.");
		}

		public Window WaitForFileDialog(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			return WaitForWindow(session, isFileDialog);
		}

		private bool isFileDialog(AutomationElement element)
		{
			AutomationElement[] comboBoxes = element.FindAllDescendants(cf => cf.ByControlType(ControlType.ComboBox));

			AutomationElement[] buttons = element.FindAllDescendants(cf => cf.ByControlType(ControlType.Button));

			bool hasFileNameInput = comboBoxes.Any(comboBox => comboBox.FindFirstDescendant(cf => cf.ByControlType(ControlType.Edit)) != null);

			bool hasOpenButton = buttons.Any(button => button.Name.StartsWith("開く", StringComparison.OrdinalIgnoreCase) || button.Name.StartsWith("Open", StringComparison.OrdinalIgnoreCase));

			return hasFileNameInput && hasOpenButton;
		}
	}
}
