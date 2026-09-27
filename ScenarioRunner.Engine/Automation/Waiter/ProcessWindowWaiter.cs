using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace ScenarioRunner.Automation.Waiter
{
	public class ProcessWindowWaiter
	{
		private const int DEFAULT_TIMEOUT_MS = 5000;
		private const int DEFAULT_INTERVAL_MS = 100;

		private const uint GW_OWNER = 4;

		private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

		[DllImport("user32.dll")]
		private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

		[DllImport("user32.dll")]
		private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

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

			AutomationElement[] windowElements = desktop.FindAllChildren(cf => cf.ByControlType(ControlType.Window).And(cf.ByProcessId(processId)));

			Window window = findWindow(windowElements, predicate);

			if (window != null)
			{
				return window;
			}

			windowElements = desktop.FindAllDescendants(cf => cf.ByControlType(ControlType.Window).And(cf.ByProcessId(processId)));

			return findWindow(windowElements, predicate);
		}

		public Window FindProcessWindow(GuiSession session, Window owner, Func<AutomationElement, bool> predicate)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			if (owner == null)
			{
				throw new ArgumentNullException(nameof(owner));
			}

			if (predicate == null)
			{
				throw new ArgumentNullException(nameof(predicate));
			}

			Window window = findNativeOwnedProcessWindow(session, owner, predicate);

			if (window != null)
			{
				return window;
			}

			window = findProcessWindow(owner, session.Application.ProcessId, predicate);

			if (window != null)
			{
				return window;
			}

			return FindProcessWindow(session, predicate);
		}

		public Window WaitForProcessWindow(GuiSession session, Window owner, Func<AutomationElement, bool> predicate)
		{
			return WaitForProcessWindow(session, owner, predicate, DEFAULT_TIMEOUT_MS, DEFAULT_INTERVAL_MS);
		}

		public Window WaitForProcessWindow(GuiSession session, Window owner, Func<AutomationElement, bool> predicate, int timeoutMs, int intervalMs)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			if (owner == null)
			{
				throw new ArgumentNullException(nameof(owner));
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
				Window window = FindProcessWindow(session, owner, predicate);

				if (window != null)
				{
					return window;
				}

				Thread.Sleep(intervalMs);
				elapsed += intervalMs;
			}

			throw new InvalidOperationException($"Process window was not found within {timeoutMs} ms.");
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

		private Window findNativeOwnedProcessWindow(GuiSession session, Window owner, Func<AutomationElement, bool> predicate)
		{
			IntPtr ownerHandle = owner.Properties.NativeWindowHandle.ValueOrDefault;

			if (ownerHandle == IntPtr.Zero)
			{
				return null;
			}

			int processId = session.Application.ProcessId;
			var windowHandles = new List<IntPtr>();

			EnumWindows((windowHandle, lParam) =>
			{
				GetWindowThreadProcessId(windowHandle, out uint windowProcessId);

				if (windowProcessId != (uint)processId)
				{
					return true;
				}

				if (GetWindow(windowHandle, GW_OWNER) != ownerHandle)
				{
					return true;
				}

				windowHandles.Add(windowHandle);

				return true;
			}, IntPtr.Zero);

			foreach (IntPtr windowHandle in windowHandles)
			{
				AutomationElement candidate;

				try
				{
					candidate = session.Automation.FromHandle(windowHandle);
				}
				catch (ElementNotAvailableException)
				{
					continue;
				}
				catch (COMException)
				{
					continue;
				}

				if (candidate != null && tryGetWindow(candidate, predicate, out Window window))
				{
					return window;
				}
			}

			return null;
		}

		private Window findProcessWindow(Window owner, int processId, Func<AutomationElement, bool> predicate)
		{
			AutomationElement[] windowElements = owner.FindAllChildren(cf => cf.ByControlType(ControlType.Window).And(cf.ByProcessId(processId)));

			Window window = findWindow(windowElements, predicate);

			if (window != null)
			{
				return window;
			}

			windowElements = owner.FindAllDescendants(cf => cf.ByControlType(ControlType.Window).And(cf.ByProcessId(processId)));

			return findWindow(windowElements, predicate);
		}

		private Window findWindow(IEnumerable<AutomationElement> elements, Func<AutomationElement, bool> predicate)
		{
			foreach (AutomationElement element in elements)
			{
				if (tryGetWindow(element, predicate, out Window window))
				{
					return window;
				}
			}

			return null;
		}

		private bool tryGetWindow(AutomationElement element, Func<AutomationElement, bool> predicate, out Window window)
		{
			window = null;

			try
			{
				if (!predicate(element))
				{
					return false;
				}

				window = element.AsWindow();

				return window != null;
			}
			catch (PropertyNotSupportedException)
			{
				return false;
			}
			catch (ElementNotAvailableException)
			{
				return false;
			}
			catch (COMException)
			{
				return false;
			}
		}
	}
}
