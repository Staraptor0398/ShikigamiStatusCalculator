using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace ScenarioRunner.Automation.Waiter
{
	public class WindowWaiter
	{
		private const int DEFAULT_TIMEOUT_MS = 5000;
		private const int DEFAULT_INTERVAL_MS = 100;

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

		private Window findProcessWindow(Window owner, int processId, Func<AutomationElement, bool> predicate)
		{
			AutomationElement windowElement = owner.FindAllChildren(cf => cf.ByControlType(ControlType.Window).And(cf.ByProcessId(processId))).FirstOrDefault(predicate);

			if (windowElement == null)
			{
				windowElement = owner.FindAllDescendants(cf => cf.ByControlType(ControlType.Window).And(cf.ByProcessId(processId))).FirstOrDefault(predicate);
			}

			return windowElement?.AsWindow();
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

			while (elapsed < DEFAULT_TIMEOUT_MS)
			{
				attempt++;

				FileDialogElements fileDialogElements = findFileDialog(owner, attempt);

				if (fileDialogElements != null)
				{
					return fileDialogElements;
				}

				fileDialogElements = findFileDialog(session, attempt);

				if (fileDialogElements != null)
				{
					return fileDialogElements;
				}

				Thread.Sleep(DEFAULT_INTERVAL_MS);
				elapsed += DEFAULT_INTERVAL_MS;
			}

			throw new InvalidOperationException($"File dialog was not found within {DEFAULT_TIMEOUT_MS} ms.");
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
			descendants = element.FindAllDescendants();

			var comboBoxes = new List<AutomationElement>();
			var comboBoxCandidates = new List<AutomationElement>();
			var editCandidates = new List<AutomationElement>();

			bool hasActionButton = false;
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

			fileNameComboBoxCandidates = comboBoxCandidates.ToArray();
			fileNameEdits = editCandidates.ToArray();

			bool isFileDialog = fileNameComboBoxCandidates.Length > 0 && hasActionButton;

			return isFileDialog;
		}
	}
}
