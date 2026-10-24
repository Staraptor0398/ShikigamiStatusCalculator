using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Exceptions;
using ScenarioRunner.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace ScenarioRunner.Automation.Waiter
{
	public class FileDialogWaiter
	{
		private const int DEFAULT_TIMEOUT_MS = 5000;
		private const int DEFAULT_INTERVAL_MS = 100;
		private const int NATIVE_FILE_DIALOG_GRACE_MS = 500;

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

		public FileDialogWaiter()
		{
		}

		public FileDialogWaiter(ScenarioLogger logger)
		{
			if (logger == null)
			{
				throw new ArgumentNullException(nameof(logger));
			}

			mLogger = logger;
		}

		public FileDialogElements WaitForFileDialog(GuiSession session, Window owner)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			if (owner == null)
			{
				throw new ArgumentNullException(nameof(owner));
			}

			int elapsed = 0;
			int attempt = 0;
			int nativeCandidateWaitElapsed = 0;
			int nativeCandidateAttempts = 0;

			var stopwatch = Stopwatch.StartNew();

			long nativeOwnerMs = 0;
			long ownerDescendantMs = 0;
			long desktopMs = 0;
			long sleepMs = 0;

			string route = "None";

			try
			{
				while (elapsed < DEFAULT_TIMEOUT_MS)
				{
					attempt++;

					long startedMs = stopwatch.ElapsedMilliseconds;

					FileDialogElements fileDialogElements;
					bool hasNativeCandidate = false;

					try
					{
						fileDialogElements = findNativeOwnedFileDialog(session, owner, attempt, out hasNativeCandidate);
					}
					finally
					{
						nativeOwnerMs += stopwatch.ElapsedMilliseconds - startedMs;
					}

					if (hasNativeCandidate)
					{
						nativeCandidateAttempts++;
					}

					if (fileDialogElements != null)
					{
						route = "NativeOwner";

						return fileDialogElements;
					}

					if (hasNativeCandidate && nativeCandidateWaitElapsed < NATIVE_FILE_DIALOG_GRACE_MS)
					{
						startedMs = stopwatch.ElapsedMilliseconds;

						try
						{
							Thread.Sleep(DEFAULT_INTERVAL_MS);
						}
						finally
						{
							sleepMs += stopwatch.ElapsedMilliseconds - startedMs;
						}

						elapsed += DEFAULT_INTERVAL_MS;
						nativeCandidateWaitElapsed += DEFAULT_INTERVAL_MS;

						continue;
					}

					if (!hasNativeCandidate)
					{
						nativeCandidateWaitElapsed = 0;
					}

					startedMs = stopwatch.ElapsedMilliseconds;

					try
					{
						fileDialogElements = findFileDialog(owner, attempt);
					}
					finally
					{
						ownerDescendantMs += stopwatch.ElapsedMilliseconds - startedMs;
					}

					if (fileDialogElements != null)
					{
						route = "OwnerDescendant";

						return fileDialogElements;
					}

					startedMs = stopwatch.ElapsedMilliseconds;

					try
					{
						fileDialogElements = findFileDialog(session, attempt);
					}
					finally
					{
						desktopMs += stopwatch.ElapsedMilliseconds - startedMs;
					}

					if (fileDialogElements != null)
					{
						route = "Desktop";

						return fileDialogElements;
					}

					startedMs = stopwatch.ElapsedMilliseconds;

					try
					{
						Thread.Sleep(DEFAULT_INTERVAL_MS);
					}
					finally
					{
						sleepMs += stopwatch.ElapsedMilliseconds - startedMs;
					}

					elapsed += DEFAULT_INTERVAL_MS;
				}

				throw new InvalidOperationException($"File dialog was not found within {DEFAULT_TIMEOUT_MS} ms.");
			}
			finally
			{
				stopwatch.Stop();

				if (mLogger != null)
				{
					try
					{
						long otherMs = stopwatch.ElapsedMilliseconds - nativeOwnerMs - ownerDescendantMs - desktopMs - sleepMs;

						mLogger.Write(
							$"[Timing] FileDialogWaiter.WaitForFileDialog " +
							$"Result={(route == "None" ? "FAIL" : "PASS")} " +
							$"Route={route} " +
							$"Attempts={attempt} " +
							$"NativeCandidateAttempts={nativeCandidateAttempts} " +
							$"TotalMs={stopwatch.ElapsedMilliseconds} " +
							$"NativeOwnerMs={nativeOwnerMs} " +
							$"OwnerDescendantMs={ownerDescendantMs} " +
							$"DesktopMs={desktopMs} " +
							$"SleepMs={sleepMs} " +
							$"OtherMs={otherMs}");
					}
					catch
					{
						// 調査ログの出力失敗でシナリオの実行結果を変更しない。
					}
				}
			}
		}

		private FileDialogElements findNativeOwnedFileDialog(GuiSession session, Window owner, int attempt, out bool hasCandidate)
		{
			var stopwatch = Stopwatch.StartNew();

			long getOwnerHandleMs = 0;
			long enumWindowsMs = 0;
			long fromHandleMs = 0;
			long candidateSelectionMs = 0;

			int handleCount = 0;
			int candidateCount = 0;

			bool found = false;

			hasCandidate = false;

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
				hasCandidate = handleCount > 0;

				var candidates = new List<AutomationElement>();

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

					if (candidate != null)
					{
						candidates.Add(candidate);
					}
				}

				candidateCount = candidates.Count;

				startedMs = stopwatch.ElapsedMilliseconds;
				FileDialogElements fileDialogElements;

				try
				{
					fileDialogElements = findBestFileDialogCandidate(candidates.ToArray(), false, "NativeOwner", attempt);
				}
				finally
				{
					candidateSelectionMs += stopwatch.ElapsedMilliseconds - startedMs;
				}

				found = fileDialogElements != null;

				return fileDialogElements;
			}
			finally
			{
				stopwatch.Stop();

				if (mLogger != null)
				{
					try
					{
						mLogger.Write(
							$"[Timing] FileDialogWaiter.NativeOwner " +
							$"Attempt={attempt} " +
							$"Found={found} " +
							$"Handles={handleCount} " +
							$"Candidates={candidateCount} " +
							$"TotalMs={stopwatch.ElapsedMilliseconds} " +
							$"GetOwnerHandleMs={getOwnerHandleMs} " +
							$"EnumWindowsMs={enumWindowsMs} " +
							$"FromHandleMs={fromHandleMs} " +
							$"CandidateSelectionMs={candidateSelectionMs}");
					}
					catch
					{
						// 計測ログの出力失敗でシナリオの実行結果を変更しない。
					}
				}
			}
		}

		private FileDialogElements findFileDialog(Window owner, int attempt)
		{
			AutomationElement[] candidates = owner.FindAllDescendants(cf => cf.ByControlType(ControlType.Window)).ToArray();

			return findBestFileDialogCandidate(candidates, false, "OwnerDescendant", attempt);
		}

		public FileDialogElements WaitForFileDialog(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			int elapsed = 0;
			int attempt = 0;

			while (elapsed < DEFAULT_TIMEOUT_MS)
			{
				attempt++;

				FileDialogElements fileDialogElements = findFileDialog(session, attempt);

				if (fileDialogElements != null)
				{
					return fileDialogElements;
				}

				Thread.Sleep(DEFAULT_INTERVAL_MS);
				elapsed += DEFAULT_INTERVAL_MS;
			}

			throw new InvalidOperationException($"File dialog was not found within {DEFAULT_TIMEOUT_MS} ms.");
		}

		private FileDialogElements findFileDialog(GuiSession session, int attempt)
		{
			int processId = session.Application.ProcessId;
			AutomationElement desktop = session.Automation.GetDesktop();

			AutomationElement[] topLevelCandidates = desktop.FindAllChildren(cf => cf.ByControlType(ControlType.Window).And(cf.ByProcessId(processId))).ToArray();

			FileDialogElements fileDialogElements = findDescendantFileDialog(desktop, processId, topLevelCandidates, attempt);

			if (fileDialogElements != null)
			{
				return fileDialogElements;
			}

			fileDialogElements = findTopLevelFileDialog(topLevelCandidates, attempt);

			return fileDialogElements;
		}

		private FileDialogElements findDescendantFileDialog(AutomationElement desktop, int processId, AutomationElement[] topLevelCandidates, int attempt)
		{
			var topLevelWindowHandles = new HashSet<IntPtr>(topLevelCandidates.Select(candidate => candidate.Properties.NativeWindowHandle.ValueOrDefault).Where(windowHandle => windowHandle != IntPtr.Zero));

			AutomationElement[] descendantCandidates = desktop
				.FindAllDescendants(cf => cf.ByControlType(ControlType.Window).And(cf.ByProcessId(processId)))
				.Where(candidate =>
				{
					IntPtr windowHandle = candidate.Properties.NativeWindowHandle.ValueOrDefault;

					return windowHandle == IntPtr.Zero || !topLevelWindowHandles.Contains(windowHandle);
				})
				.ToArray();

			FileDialogElements fileDialogElements = findBestFileDialogCandidate(descendantCandidates, false, "Descendant", attempt);

			return fileDialogElements;
		}

		private FileDialogElements findTopLevelFileDialog(AutomationElement[] topLevelCandidates, int attempt)
		{
			FileDialogElements fileDialogElements = findBestFileDialogCandidate(topLevelCandidates, true, "TopLevel", attempt);

			return fileDialogElements;
		}

		private FileDialogElements findBestFileDialogCandidate(AutomationElement[] candidates, bool requireNoDescendantWindow, string candidateGroup, int attempt)
		{
			AutomationElement fileDialog = null;
			AutomationElement[] fileDialogDescendants = null;
			AutomationElement[] fileNameComboBoxCandidates = null;
			AutomationElement[] fileNameEdits = null;
			int minimumDescendantWindowCount = int.MaxValue;

			for (int i = 0; i < candidates.Length; i++)
			{
				if (!tryInspectFileDialogCandidate(candidates[i], candidateGroup, i, attempt, out AutomationElement[] descendants, out AutomationElement[] comboBoxCandidates, out AutomationElement[] editCandidates, out int descendantWindowCount))
				{
					continue;
				}

				if (requireNoDescendantWindow && descendantWindowCount != 0)
				{
					continue;
				}

				if (descendantWindowCount >= minimumDescendantWindowCount)
				{
					continue;
				}

				fileDialog = candidates[i];
				fileDialogDescendants = descendants;
				fileNameComboBoxCandidates = comboBoxCandidates;
				fileNameEdits = editCandidates;
				minimumDescendantWindowCount = descendantWindowCount;

				if (descendantWindowCount == 0)
				{
					break;
				}
			}

			if (fileDialog == null)
			{
				return null;
			}

			return new FileDialogElements(fileDialog.AsWindow(), fileDialogDescendants, fileNameComboBoxCandidates, fileNameEdits);
		}

		private bool tryInspectFileDialogCandidate(AutomationElement element, string candidateGroup, int candidateIndex, int attempt, out AutomationElement[] descendants, out AutomationElement[] fileNameComboBoxCandidates, out AutomationElement[] fileNameEdits, out int descendantWindowCount)
		{
			var stopwatch = Stopwatch.StartNew();

			long findDescendantsMs = 0;
			long scanDescendantsMs = 0;
			long findEditControlsMs = 0;

			bool isFileDialog = false;

			try
			{
				long startedMs = stopwatch.ElapsedMilliseconds;

				try
				{
					descendants = element.FindAllDescendants();
				}
				finally
				{
					findDescendantsMs += stopwatch.ElapsedMilliseconds - startedMs;
				}

				var comboBoxes = new List<AutomationElement>();
				var comboBoxCandidates = new List<AutomationElement>();
				var editCandidates = new List<AutomationElement>();

				bool hasActionButton = false;
				descendantWindowCount = 0;

				startedMs = stopwatch.ElapsedMilliseconds;

				try
				{
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

							if (name.StartsWith("開く", StringComparison.OrdinalIgnoreCase) || name.StartsWith("Open", StringComparison.OrdinalIgnoreCase) || name.StartsWith("保存", StringComparison.OrdinalIgnoreCase) || name.StartsWith("Save", StringComparison.OrdinalIgnoreCase))
							{
								hasActionButton = true;
							}

							continue;
						}

						if (controlType == ControlType.Window)
						{
							descendantWindowCount++;
						}
					}
				}
				finally
				{
					scanDescendantsMs += stopwatch.ElapsedMilliseconds - startedMs;
				}

				startedMs = stopwatch.ElapsedMilliseconds;

				try
				{
					foreach (AutomationElement comboBox in comboBoxes)
					{
						AutomationElement edit = comboBox.FindFirstDescendant(cf => cf.ByControlType(ControlType.Edit));

						if (edit == null)
						{
							continue;
						}

						comboBoxCandidates.Add(comboBox);
						editCandidates.Add(edit);
					}
				}
				finally
				{
					findEditControlsMs += stopwatch.ElapsedMilliseconds - startedMs;
				}

				fileNameComboBoxCandidates = comboBoxCandidates.ToArray();
				fileNameEdits = editCandidates.ToArray();

				isFileDialog = fileNameComboBoxCandidates.Length > 0 && hasActionButton;

				return isFileDialog;
			}
			finally
			{
				stopwatch.Stop();

				if (mLogger != null && candidateGroup == "NativeOwner")
				{
					try
					{
						mLogger.Write(
							$"[Timing] FileDialogWaiter.InspectCandidate " +
							$"Attempt={attempt} " +
							$"CandidateIndex={candidateIndex} " +
							$"IsFileDialog={isFileDialog} " +
							$"TotalMs={stopwatch.ElapsedMilliseconds} " +
							$"FindDescendantsMs={findDescendantsMs} " +
							$"ScanDescendantsMs={scanDescendantsMs} " +
							$"FindEditControlsMs={findEditControlsMs}");
					}
					catch
					{
						// 計測ログの出力失敗でシナリオの実行結果を変更しない。
					}
				}
			}
		}
	}
}
