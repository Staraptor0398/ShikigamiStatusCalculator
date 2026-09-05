using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace ScenarioRunner.Automation.Waiter
{
	public class WindowWaiter
	{
		private const int DEFAULT_TIMEOUT_MS = 5000;
		private const int DEFAULT_INTERVAL_MS = 100;
		private const int DEFAULT_WAIT_INTERVAL_MS = 50;

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

			AutomationElement windowElement = desktop.FindAllDescendants(cf => cf.ByControlType(ControlType.Window)).FirstOrDefault(predicate);

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

		public Window WaitForWindow(GuiSession session, Func<Window, bool> predicate, CancellationToken cancellationToken)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			if (predicate == null)
			{
				throw new ArgumentNullException(nameof(predicate));
			}

			while (true)
			{
				cancellationToken.ThrowIfCancellationRequested();

				Window window = findWindow(session, predicate);

				if (window != null)
				{
					return window;
				}

				Thread.Sleep(DEFAULT_WAIT_INTERVAL_MS);
			}
		}

		public Window WaitForFileDialog(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			return WaitForWindow(session, isFileDialog);
		}

		private Window findWindow(GuiSession session, Func<Window, bool> predicate)
		{
			return session.Application
			.GetAllTopLevelWindows(session.Automation)
			.FirstOrDefault(predicate);
		}

		private bool isFileDialog(AutomationElement element)
		{
			AutomationElement[] comboBoxes = element.FindAllDescendants(cf => cf.ByControlType(ControlType.ComboBox));

			AutomationElement[] buttons = element.FindAllDescendants(cf => cf.ByControlType(ControlType.Button));

			bool hasFileNameInput = comboBoxes.Any(comboBox => comboBox.FindFirstDescendant(cf => cf.ByControlType(ControlType.Edit)) != null);
			bool hasOpenButton = buttons.Any(button => button.Name.StartsWith("開く", StringComparison.OrdinalIgnoreCase) || button.Name.StartsWith("Open", StringComparison.OrdinalIgnoreCase));

			return hasFileNameInput && hasOpenButton;
		}

		private void dumpWindows(Window[] windows)
		{
			string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WindowDump.txt");

			using (StreamWriter writer = new StreamWriter(filePath, true))
			{
				writer.WriteLine("========================================");
				writer.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));

				foreach (Window window in windows)
				{
					writer.WriteLine(
					$"Name={window.Properties.Name.ValueOrDefault}, " +
					$"AutomationId={window.Properties.AutomationId.ValueOrDefault}, " +
					$"ControlType={window.Properties.ControlType.ValueOrDefault}, " +
					$"ClassName={window.Properties.ClassName.ValueOrDefault}, " +
					$"ProcessId={window.Properties.ProcessId.ValueOrDefault}");
				}

				writer.WriteLine();
			}
		}

		private void dumpAutomationElements(AutomationElement[] elements)
		{
			string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WindowDump.txt");

			using (StreamWriter writer = new StreamWriter(filePath, true))
			{
				writer.WriteLine("========================================");
				writer.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));

				foreach (AutomationElement element in elements)
				{
					writer.WriteLine(
					$"Name={element.Properties.Name.ValueOrDefault}, " +
					$"AutomationId={element.Properties.AutomationId.ValueOrDefault}, " +
					$"ControlType={element.Properties.ControlType.ValueOrDefault}, " +
					$"ClassName={element.Properties.ClassName.ValueOrDefault}, " +
					$"ProcessId={element.Properties.ProcessId.ValueOrDefault}");
				}

				writer.WriteLine();
			}
		}
	}
}
