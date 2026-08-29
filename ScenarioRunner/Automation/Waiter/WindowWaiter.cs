using FlaUI.Core.AutomationElements;
using System;
using System.Linq;
using System.Threading;

namespace ScenarioRunner.Automation.Waiter
{
	public class WindowWaiter
	{
		private const int DEFAULT_WAIT_INTERVAL_MS = 50;

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

		private Window findWindow(GuiSession session, Func<Window, bool> predicate)
		{
			return session.Application.GetAllTopLevelWindows(session.Automation).FirstOrDefault(predicate);
		}
	}
}
