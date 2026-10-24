using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Exceptions;
using ScenarioRunner.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

		private readonly ScenarioLogger mLogger;

		public ProcessWindowWaiter()
		{
		}

		public ProcessWindowWaiter(ScenarioLogger logger)
		{
			if (logger == null)
			{
				throw new ArgumentNullException(nameof(logger));
			}

			mLogger = logger;
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

			Window window = findChildProcessWindow(desktop, processId, predicate);

			if (window != null)
			{
				return window;
			}

			return findDescendantProcessWindow(desktop, processId, predicate);
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

			var stopwatch = Stopwatch.StartNew();

			long nativeOwnerMs = 0;
			long ownerMs = 0;
			long desktopMs = 0;

			string route = "None";

			try
			{
				long startedMs = stopwatch.ElapsedMilliseconds;
				Window window;

				try
				{
					window = findNativeOwnedProcessWindow(session, owner, predicate);
				}
				finally
				{
					nativeOwnerMs += stopwatch.ElapsedMilliseconds - startedMs;
				}

				if (window != null)
				{
					route = "NativeOwner";

					return window;
				}

				startedMs = stopwatch.ElapsedMilliseconds;

				try
				{
					window = findProcessWindow(owner, session.Application.ProcessId, predicate);
				}
				finally
				{
					ownerMs += stopwatch.ElapsedMilliseconds - startedMs;
				}

				if (window != null)
				{
					route = "Owner";

					return window;
				}

				startedMs = stopwatch.ElapsedMilliseconds;

				try
				{
					window = FindProcessWindow(session, predicate);
				}
				finally
				{
					desktopMs += stopwatch.ElapsedMilliseconds - startedMs;
				}

				if (window != null)
				{
					route = "Desktop";
				}

				return window;
			}
			finally
			{
				stopwatch.Stop();

				writeTiming(
					$"[Timing] ProcessWindowWaiter.FindProcessWindow " +
					$"Route={route} " +
					$"TotalMs={stopwatch.ElapsedMilliseconds} " +
					$"NativeOwnerMs={nativeOwnerMs} " +
					$"OwnerMs={ownerMs} " +
					$"DesktopMs={desktopMs}");
			}
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
			int attempts = 0;

			long searchMs = 0;
			long sleepMs = 0;

			bool completed = false;

			var stopwatch = Stopwatch.StartNew();

			try
			{
				while (elapsed < timeoutMs)
				{
					attempts++;

					long startedMs = stopwatch.ElapsedMilliseconds;
					Window window;

					try
					{
						window = FindProcessWindow(session, owner, predicate);
					}
					finally
					{
						searchMs += stopwatch.ElapsedMilliseconds - startedMs;
					}

					if (window != null)
					{
						completed = true;

						return window;
					}

					startedMs = stopwatch.ElapsedMilliseconds;

					try
					{
						Thread.Sleep(intervalMs);
					}
					finally
					{
						sleepMs += stopwatch.ElapsedMilliseconds - startedMs;
					}

					elapsed += intervalMs;
				}

				throw new InvalidOperationException($"Process window was not found within {timeoutMs} ms.");
			}
			finally
			{
				stopwatch.Stop();

				writeTiming(
					$"[Timing] ProcessWindowWaiter.WaitForProcessWindow " +
					$"Result={(completed ? "PASS" : "FAIL")} " +
					$"Attempts={attempts} " +
					$"TotalMs={stopwatch.ElapsedMilliseconds} " +
					$"SearchMs={searchMs} " +
					$"SleepMs={sleepMs}");
			}
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
			var stopwatch = Stopwatch.StartNew();

			long getOwnerHandleMs = 0;
			long enumWindowsMs = 0;
			long fromHandleMs = 0;
			long inspectCandidateMs = 0;

			int handleCount = 0;
			int candidateCount = 0;

			bool found = false;

			try
			{
				long startedMs = stopwatch.ElapsedMilliseconds;
				IntPtr ownerHandle;

				try
				{
					ownerHandle = owner.Properties.NativeWindowHandle.ValueOrDefault;
				}
				finally
				{
					getOwnerHandleMs += stopwatch.ElapsedMilliseconds - startedMs;
				}

				if (ownerHandle == IntPtr.Zero)
				{
					return null;
				}

				int processId = session.Application.ProcessId;
				var windowHandles = new List<IntPtr>();

				startedMs = stopwatch.ElapsedMilliseconds;

				try
				{
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
				}
				finally
				{
					enumWindowsMs += stopwatch.ElapsedMilliseconds - startedMs;
				}

				handleCount = windowHandles.Count;

				foreach (IntPtr windowHandle in windowHandles)
				{
					AutomationElement candidate = null;

					startedMs = stopwatch.ElapsedMilliseconds;

					try
					{
						try
						{
							candidate = session.Automation.FromHandle(windowHandle);
						}
						catch (PropertyNotSupportedException)
						{
							continue;
						}
						catch (ElementNotAvailableException)
						{
							continue;
						}
						catch (System.Windows.Automation.ElementNotAvailableException)
						{
							continue;
						}
						catch (COMException)
						{
							continue;
						}
					}
					finally
					{
						fromHandleMs += stopwatch.ElapsedMilliseconds - startedMs;
					}

					if (candidate == null)
					{
						continue;
					}

					candidateCount++;

					startedMs = stopwatch.ElapsedMilliseconds;

					try
					{
						if (tryGetNativeWindow(candidate, predicate, out Window window))
						{
							found = true;

							return window;
						}
					}
					finally
					{
						inspectCandidateMs += stopwatch.ElapsedMilliseconds - startedMs;
					}
				}

				return null;
			}
			finally
			{
				stopwatch.Stop();

				writeTiming(
					$"[Timing] ProcessWindowWaiter.NativeOwner " +
					$"Found={found} " +
					$"Handles={handleCount} " +
					$"Candidates={candidateCount} " +
					$"TotalMs={stopwatch.ElapsedMilliseconds} " +
					$"GetOwnerHandleMs={getOwnerHandleMs} " +
					$"EnumWindowsMs={enumWindowsMs} " +
					$"FromHandleMs={fromHandleMs} " +
					$"InspectCandidateMs={inspectCandidateMs}");
			}
		}

		private Window findProcessWindow(Window owner, int processId, Func<AutomationElement, bool> predicate)
		{
			Window window = findChildProcessWindow(owner, processId, predicate);

			if (window != null)
			{
				return window;
			}

			return findDescendantProcessWindow(owner, processId, predicate);
		}

		private Window findChildProcessWindow(AutomationElement parent, int processId, Func<AutomationElement, bool> predicate)
		{
			try
			{
				AutomationElement[] windowElements = parent.FindAllChildren(cf => cf.ByControlType(ControlType.Window).And(cf.ByProcessId(processId)));

				return findWindow(windowElements, predicate);
			}
			catch (PropertyNotSupportedException)
			{
				return null;
			}
			catch (ElementNotAvailableException)
			{
				return null;
			}
			catch (System.Windows.Automation.ElementNotAvailableException)
			{
				return null;
			}
			catch (COMException)
			{
				return null;
			}
		}

		private Window findDescendantProcessWindow(AutomationElement parent, int processId, Func<AutomationElement, bool> predicate)
		{
			try
			{
				AutomationElement[] windowElements = parent.FindAllDescendants(cf => cf.ByControlType(ControlType.Window).And(cf.ByProcessId(processId)));

				return findWindow(windowElements, predicate);
			}
			catch (PropertyNotSupportedException)
			{
				return null;
			}
			catch (ElementNotAvailableException)
			{
				return null;
			}
			catch (System.Windows.Automation.ElementNotAvailableException)
			{
				return null;
			}
			catch (COMException)
			{
				return null;
			}
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

		private bool tryGetNativeWindow(AutomationElement element, Func<AutomationElement, bool> predicate, out Window window)
		{
			window = null;

			var stopwatch = Stopwatch.StartNew();

			long matchWindowMs = 0;
			long findDescendantsMs = 0;

			bool matched = false;
			bool completed = false;

			try
			{
				long startedMs = stopwatch.ElapsedMilliseconds;
				Window candidate;

				try
				{
					matched = tryGetWindow(element, predicate, out candidate);
				}
				finally
				{
					matchWindowMs += stopwatch.ElapsedMilliseconds - startedMs;
				}

				if (!matched)
				{
					return false;
				}

				startedMs = stopwatch.ElapsedMilliseconds;

				try
				{
					element.FindAllDescendants();

					window = candidate;
					completed = true;

					return true;
				}
				catch (PropertyNotSupportedException)
				{
					return false;
				}
				catch (ElementNotAvailableException)
				{
					return false;
				}
				catch (System.Windows.Automation.ElementNotAvailableException)
				{
					return false;
				}
				catch (COMException)
				{
					return false;
				}
				finally
				{
					findDescendantsMs += stopwatch.ElapsedMilliseconds - startedMs;
				}
			}
			finally
			{
				stopwatch.Stop();

				writeTiming(
					$"[Timing] ProcessWindowWaiter.TryGetNativeWindow " +
					$"Matched={matched} " +
					$"Result={(completed ? "PASS" : "FAIL")} " +
					$"TotalMs={stopwatch.ElapsedMilliseconds} " +
					$"MatchWindowMs={matchWindowMs} " +
					$"FindDescendantsMs={findDescendantsMs}");
			}
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
			catch (System.Windows.Automation.ElementNotAvailableException)
			{
				return false;
			}
			catch (COMException)
			{
				return false;
			}
		}

		private void writeTiming(string message)
		{
			if (mLogger == null)
			{
				return;
			}

			try
			{
				mLogger.Write(message);
			}
			catch
			{
				// 計測ログの出力失敗でシナリオの実行結果を変更しない。
			}
		}
	}
}
