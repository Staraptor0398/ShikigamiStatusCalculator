using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using System;
using System.Linq;
using System.Threading;

namespace ScenarioRunner.Automation.Waiter
{
	public class ProcessWindowWaiter
	{
		private const int DEFAULT_TIMEOUT_MS = 5000;
		private const int DEFAULT_INTERVAL_MS = 100;

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

			Window window = findProcessWindow(owner, session.Application.ProcessId, predicate);

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

		private Window findProcessWindow(Window owner, int processId, Func<AutomationElement, bool> predicate)
		{
			AutomationElement windowElement = owner.FindAllChildren(cf => cf.ByControlType(ControlType.Window).And(cf.ByProcessId(processId))).FirstOrDefault(predicate);

			if (windowElement == null)
			{
				windowElement = owner.FindAllDescendants(cf => cf.ByControlType(ControlType.Window).And(cf.ByProcessId(processId))).FirstOrDefault(predicate);
			}

			return windowElement?.AsWindow();
		}
	}
}
