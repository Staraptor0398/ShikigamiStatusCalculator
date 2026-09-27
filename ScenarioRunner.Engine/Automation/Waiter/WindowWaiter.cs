using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace ScenarioRunner.Automation.Waiter
{
	public class WindowWaiter
	{
		private const int DEFAULT_TIMEOUT_MS = 5000;
		private const int DEFAULT_INTERVAL_MS = 100;
		private const int WINDOW_CLOSED_INTERVAL_MS = 50;

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

		public void WaitForWindowClosed(Window window)
		{
			WaitForWindowClosed(window, DEFAULT_TIMEOUT_MS, WINDOW_CLOSED_INTERVAL_MS);
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
	}
}
