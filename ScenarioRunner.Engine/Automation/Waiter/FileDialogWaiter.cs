using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using System;
using System.Collections.Generic;
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

			while (elapsed < DEFAULT_TIMEOUT_MS)
			{
				attempt++;

				FileDialogElements fileDialogElements = findNativeOwnedFileDialog(session, owner, attempt, out bool hasNativeCandidate);

				if (fileDialogElements != null)
				{
					return fileDialogElements;
				}

				if (hasNativeCandidate && nativeCandidateWaitElapsed < NATIVE_FILE_DIALOG_GRACE_MS)
				{
					Thread.Sleep(DEFAULT_INTERVAL_MS);

					elapsed += DEFAULT_INTERVAL_MS;
					nativeCandidateWaitElapsed += DEFAULT_INTERVAL_MS;

					continue;
				}

				if (!hasNativeCandidate)
				{
					nativeCandidateWaitElapsed = 0;
				}

				fileDialogElements = findFileDialog(owner, attempt);

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

		private FileDialogElements findNativeOwnedFileDialog(GuiSession session, Window owner, int attempt, out bool hasCandidate)
		{
			IntPtr ownerHandle = owner.Properties.NativeWindowHandle.ValueOrDefault;

			if (ownerHandle == IntPtr.Zero)
			{
				hasCandidate = false;

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

			hasCandidate = windowHandles.Count > 0;

			var candidates = new List<AutomationElement>();

			foreach (IntPtr windowHandle in windowHandles)
			{
				AutomationElement candidate = session.Automation.FromHandle(windowHandle);

				if (candidate != null)
				{
					candidates.Add(candidate);
				}
			}

			FileDialogElements fileDialogElements = findBestFileDialogCandidate(candidates.ToArray(), false, "NativeOwner", attempt);

			return fileDialogElements;
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
