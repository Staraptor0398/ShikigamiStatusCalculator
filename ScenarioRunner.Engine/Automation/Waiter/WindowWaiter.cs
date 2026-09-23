using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace ScenarioRunner.Automation.Waiter
{
	public class WindowWaiter
	{
		private const int DEFAULT_TIMEOUT_MS = 5000;
		private const int DEFAULT_INTERVAL_MS = 100;
		private const int DEFAULT_WAIT_INTERVAL_MS = 50;

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool IsWindow(IntPtr hWnd);

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

			if (windowElement == null)
			{
				windowElement = desktop.FindAllDescendants(cf => cf.ByControlType(ControlType.Window)).FirstOrDefault(predicate);
			}

			return windowElement?.AsWindow();
		}

		public bool Exists(GuiSession session, Func<AutomationElement, bool> predicate)
		{
			return FindWindow(session, predicate) != null;
		}

		public Window FindProcessWindow(GuiSession session, Func<AutomationElement, bool> predicate)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			if (predicate == null)
			{
				throw new ArgumentNullException(nameof(predicate));
			}

			int processId = session.Application.ProcessId;
			AutomationElement desktop = session.Automation.GetDesktop();

			AutomationElement windowElement = desktop.FindAllChildren(cf => cf.ByControlType(ControlType.Window).And(cf.ByProcessId(processId))).FirstOrDefault(predicate);

			if (windowElement == null)
			{
				windowElement = desktop.FindAllDescendants(cf => cf.ByControlType(ControlType.Window).And(cf.ByProcessId(processId))).FirstOrDefault(predicate);
			}

			return windowElement?.AsWindow();
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

		public Window WaitForProcessWindow(GuiSession session, Func<AutomationElement, bool> predicate)
		{
			return WaitForProcessWindow(session, predicate, DEFAULT_TIMEOUT_MS, DEFAULT_INTERVAL_MS);
		}

		public Window WaitForProcessWindow(GuiSession session, Func<AutomationElement, bool> predicate, int timeoutMs, int intervalMs)
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
				Window window = FindProcessWindow(session, predicate);

				if (window != null)
				{
					return window;
				}

				Thread.Sleep(intervalMs);
				elapsed += intervalMs;
			}

			throw new InvalidOperationException($"Process window was not found within {timeoutMs} ms.");
		}

		public void WaitForWindowClosed(GuiSession session, Func<AutomationElement, bool> predicate)
		{
			WaitForWindowClosed(session, predicate, DEFAULT_TIMEOUT_MS, DEFAULT_INTERVAL_MS);
		}

		public void WaitForWindowClosed(GuiSession session, Func<AutomationElement, bool> predicate, int timeoutMs, int intervalMs)
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
				if (!Exists(session, predicate))
				{
					return;
				}

				Thread.Sleep(intervalMs);
				elapsed += intervalMs;
			}

			throw new InvalidOperationException($"Window was not closed within {timeoutMs} ms.");
		}

		public void WaitForWindowClosed(Window window)
		{
			WaitForWindowClosed(window, DEFAULT_TIMEOUT_MS, DEFAULT_INTERVAL_MS);
		}

		public void WaitForWindowClosed(Window window, int timeoutMs, int intervalMs)
		{
			if (window == null)
			{
				throw new ArgumentNullException(nameof(window));
			}

			if (timeoutMs <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(timeoutMs));
			}

			if (intervalMs <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(intervalMs));
			}

			IntPtr windowHandle = window.Properties.NativeWindowHandle.ValueOrDefault;

			if (windowHandle == IntPtr.Zero)
			{
				throw new InvalidOperationException("Window has no native window handle.");
			}

			int elapsed = 0;

			while (elapsed < timeoutMs)
			{
				if (!IsWindow(windowHandle))
				{
					return;
				}

				Thread.Sleep(intervalMs);
				elapsed += intervalMs;
			}

			throw new InvalidOperationException($"Window was not closed within {timeoutMs} ms.");
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

			int elapsed = 0;

			while (elapsed < DEFAULT_TIMEOUT_MS)
			{
				Window fileDialog = findFileDialog(session);

				if (fileDialog != null)
				{
					return fileDialog;
				}

				Thread.Sleep(DEFAULT_INTERVAL_MS);
				elapsed += DEFAULT_INTERVAL_MS;
			}

			throw new InvalidOperationException($"File dialog was not found within {DEFAULT_TIMEOUT_MS} ms.");
		}

		private Window findWindow(GuiSession session, Func<Window, bool> predicate)
		{
			return session.Application.GetAllTopLevelWindows(session.Automation).FirstOrDefault(predicate);
		}

		private Window findFileDialog(GuiSession session)
		{
			int processId = session.Application.ProcessId;
			AutomationElement desktop = session.Automation.GetDesktop();

			AutomationElement[] candidates = desktop.FindAllDescendants(cf => cf.ByControlType(ControlType.Window).And(cf.ByProcessId(processId))).ToArray();

			AutomationElement fileDialog = null;
			int minimumDescendantWindowCount = int.MaxValue;

			foreach (AutomationElement candidate in candidates)
			{
				if (!tryInspectFileDialog(candidate, out int descendantWindowCount))
				{
					continue;
				}

				if (descendantWindowCount >= minimumDescendantWindowCount)
				{
					continue;
				}

				fileDialog = candidate;
				minimumDescendantWindowCount = descendantWindowCount;
			}

			return fileDialog?.AsWindow();
		}

		private bool tryInspectFileDialog(AutomationElement element, out int descendantWindowCount)
		{
			AutomationElement[] descendants = element.FindAllDescendants();

			var comboBoxes = new List<AutomationElement>();
			bool hasOpenButton = false;
			descendantWindowCount = 0;

			foreach (AutomationElement descendant in descendants)
			{
				ControlType controlType = descendant.Properties.ControlType.ValueOrDefault;

				if (controlType == ControlType.ComboBox)
				{
					comboBoxes.Add(descendant);
					continue;
				}

				if (controlType == ControlType.Button)
				{
					string name = descendant.Properties.Name.ValueOrDefault;

					if (name.StartsWith("開く", StringComparison.OrdinalIgnoreCase) || name.StartsWith("Open", StringComparison.OrdinalIgnoreCase))
					{
						hasOpenButton = true;
					}

					continue;
				}

				if (controlType == ControlType.Window)
				{
					descendantWindowCount++;
				}
			}

			bool hasFileNameInput = comboBoxes.Any(comboBox => comboBox.FindFirstDescendant(cf => cf.ByControlType(ControlType.Edit)) != null);

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
