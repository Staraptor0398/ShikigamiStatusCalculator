
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Exceptions;
using ScenarioRunner.Automation.Definition;
using ScenarioRunner.Automation.Waiter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace ScenarioRunner.Automation.Operator.Feature
{
	public class DialogOperator
	{
		private const int MESSAGE_BOX_TIMEOUT_MS = 1000;
		private const int MESSAGE_BOX_INTERVAL_MS = 100;

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool IsWindowVisible(IntPtr hWnd);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool IsWindowEnabled(IntPtr hWnd);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool IsWindow(IntPtr hWnd);

		private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

		[DllImport("user32.dll")]
		private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern int GetClassName(IntPtr hWnd, System.Text.StringBuilder lpClassName, int nMaxCount);

		private readonly ButtonOperator mButtonOperator;
		private readonly GuiOperator mGuiOperator;

		private readonly WindowWaiter mWindowWaiter;
		private readonly ProcessWindowWaiter mProcessWindowWaiter;

		public DialogOperator()
		{
			mButtonOperator = new ButtonOperator();
			mGuiOperator = new GuiOperator();

			mWindowWaiter = new WindowWaiter();
			mProcessWindowWaiter = new ProcessWindowWaiter();
		}

		public Window GetActiveDialog(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			session.LastCheckedDialog = null;
			session.LastCheckedDialogButtons = null;

			Window mainWindow = session.MainWindow;
			IntPtr mainWindowHandle = mainWindow?.Properties.NativeWindowHandle.ValueOrDefault ?? IntPtr.Zero;

			AutomationElement[] buttons = null;

			Func<AutomationElement, bool> predicate = element =>
			{
				IntPtr windowHandle = element.Properties.NativeWindowHandle.ValueOrDefault;
				string automationId = element.Properties.AutomationId.ValueOrDefault;

				if ((mainWindowHandle != IntPtr.Zero && windowHandle == mainWindowHandle)
					|| automationId == AutomationIds.MainForm.ID
					|| !isVisible(windowHandle))
				{
					return false;
				}

				if (!tryInspectDialog(element, out AutomationElement[] candidateButtons, out AutomationElement[] texts))
				{
					return false;
				}

				if (candidateButtons.Length == 0 || texts.Length == 0)
				{
					return false;
				}

				buttons = candidateButtons;

				return true;
			};

			Window dialog;

			if (mainWindow != null)
			{
				dialog = mProcessWindowWaiter.WaitForProcessWindow(session, mainWindow, predicate);
			}
			else
			{
				dialog = mProcessWindowWaiter.WaitForProcessWindow(session, predicate);
			}

			session.LastCheckedDialog = dialog;
			session.LastCheckedDialogButtons = buttons;

			return dialog;
		}

		public bool Exists(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			Window mainWindow = mGuiOperator.GetMainWindow(session);
			IntPtr mainWindowHandle = mainWindow.Properties.NativeWindowHandle.Value;
			uint processId = (uint)session.Application.ProcessId;

			bool dialogExists = false;

			EnumWindows((windowHandle, lParam) =>
			{
				GetWindowThreadProcessId(windowHandle, out uint windowProcessId);

				if (windowProcessId != processId || windowHandle == mainWindowHandle || !isVisible(windowHandle))
				{
					return true;
				}

				AutomationElement element;

				try
				{
					element = session.Automation.FromHandle(windowHandle);
				}
				catch (PropertyNotSupportedException)
				{
					return true;
				}
				catch (ElementNotAvailableException)
				{
					return true;
				}
				catch (System.Windows.Automation.ElementNotAvailableException)
				{
					return true;
				}
				catch (COMException)
				{
					return true;
				}

				if (element == null || !tryInspectDialog(element, out AutomationElement[] buttons, out AutomationElement[] texts) || buttons.Length == 0 || texts.Length == 0)
				{
					return true;
				}

				dialogExists = true;

				return false;
			}, IntPtr.Zero);

			return dialogExists;
		}

		public string GetMessage(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			Window dialog = session.LastCheckedDialog ?? GetActiveDialog(session);

			if (!tryInspectDialog(dialog, out AutomationElement[] buttons, out AutomationElement[] texts))
			{
				return null;
			}

			return texts.Select(element => element.Properties.Name.ValueOrDefault).FirstOrDefault(text => !string.IsNullOrWhiteSpace(text));
		}

		public void CheckMessage(GuiSession session, string expectedMessage)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			if (string.IsNullOrWhiteSpace(expectedMessage))
			{
				throw new ArgumentException("Expected dialog message is empty.", nameof(expectedMessage));
			}

			session.LastCheckedDialog = null;
			session.LastCheckedDialogButtons = null;

			Window mainWindow = session.MainWindow;
			IntPtr mainWindowHandle = mainWindow?.Properties.NativeWindowHandle.ValueOrDefault ?? IntPtr.Zero;

			Func<AutomationElement, bool> predicate = element =>
			{
				IntPtr windowHandle = element.Properties.NativeWindowHandle.ValueOrDefault;

				string automationId = element.Properties.AutomationId.ValueOrDefault;

				if ((mainWindowHandle != IntPtr.Zero && windowHandle == mainWindowHandle) || automationId == AutomationIds.MainForm.ID || !isVisible(windowHandle))
				{
					return false;
				}

				if (!tryInspectDialog(element, out AutomationElement[] buttons, out AutomationElement[] texts))
				{
					return false;
				}

				bool containsExpectedMessage = texts.Any(text =>
				{
					string name = text.Properties.Name.ValueOrDefault;

					return !string.IsNullOrWhiteSpace(name) && name.Contains(expectedMessage);
				});

				if (!containsExpectedMessage)
				{
					return false;
				}

				session.LastCheckedDialogButtons = buttons;

				return true;
			};

			if (waitForMessageBox(session, mainWindowHandle, expectedMessage, out Window messageBox, out AutomationElement[] messageBoxButtons))
			{
				session.LastCheckedDialog = messageBox;
				session.LastCheckedDialogButtons = messageBoxButtons;
				return;
			}

			if (mainWindow != null)
			{
				session.LastCheckedDialog = mProcessWindowWaiter.WaitForProcessWindow(session, mainWindow, predicate);
			}
			else
			{
				session.LastCheckedDialog = mProcessWindowWaiter.WaitForProcessWindow(session, predicate);
			}
		}

		public void Confirm(GuiSession session)
		{
			executeDecision(session, true);
		}

		public void Cancel(GuiSession session)
		{
			executeDecision(session, false);
		}

		private void executeDecision(GuiSession session, bool confirm)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			string operation = confirm ? "Confirm" : "Cancel";

			try
			{
				Window dialog = getDecisionDialog(session);
				string automationId = getDecisionButtonId(dialog, confirm);

				if (automationId == null)
				{
					throw new InvalidOperationException($"{operation} operation is not defined for this dialog.");
				}

				IntPtr handle = dialog.Properties.NativeWindowHandle.ValueOrDefault;
				mButtonOperator.PostClick(dialog, automationId);
				mWindowWaiter.WaitForWindowClosed(handle);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"DialogOperator.{operation} failed: {ex.Message}", ex);
			}
			finally
			{
				// 操作の成功・失敗にかかわらず、古いAutomationElementをキャッシュに残さない。
				session.LastCheckedDialog = null;
				session.LastCheckedDialogButtons = null;
			}
		}

		private Window getDecisionDialog(GuiSession session)
		{
			Window cached = session.LastCheckedDialog;

			if (cached != null)
			{
				try
				{
					IntPtr handle = cached.Properties.NativeWindowHandle.ValueOrDefault;

					if (IsWindow(handle) && isVisible(handle) && IsWindowEnabled(handle))
					{
						return cached;
					}
				}
				catch (ElementNotAvailableException)
				{
				}
				catch (System.Windows.Automation.ElementNotAvailableException)
				{
				}
				catch (COMException)
				{
				}
			}

			Window mainWindow = session.MainWindow;

			Func<AutomationElement, bool> predicate = element =>
			{
				IntPtr handle = element.Properties.NativeWindowHandle.ValueOrDefault;

				if (handle == IntPtr.Zero || !isVisible(handle) || !IsWindowEnabled(handle))
				{
					return false;
				}

				string id = element.Properties.AutomationId.ValueOrDefault;

				if (id == AutomationIds.MainForm.ID)
				{
					return false;
				}

				return isMessageBox(handle) || isKnownDecisionDialog(id);
			};

			return mainWindow != null
				? mProcessWindowWaiter.WaitForProcessWindow(session, mainWindow, predicate)
				: mProcessWindowWaiter.WaitForProcessWindow(session, predicate);
		}

		private bool isKnownDecisionDialog(string id)
		{
			return id == AutomationIds.SaveDataSaveDialog.ID
				|| id == AutomationIds.SaveDataLoadDialog.ID
				|| id == AutomationIds.ShikigamiRegisterForm.ID
				|| id == AutomationIds.ShikigamiRecoveryDialog.ID
				|| id == AutomationIds.SnapshotCompareFileSelectDialog.ID;
		}

		private string getDecisionButtonId(Window dialog, bool confirm)
		{
			IntPtr handle = dialog.Properties.NativeWindowHandle.ValueOrDefault;
			if (isMessageBox(handle))
			{
				if (confirm)
				{
					return hasButton(dialog, AutomationIds.MessageBox.YES) ? AutomationIds.MessageBox.YES
						: hasButton(dialog, AutomationIds.MessageBox.OK) ? AutomationIds.MessageBox.OK : null;
				}

				return hasButton(dialog, AutomationIds.MessageBox.CANCEL) ? AutomationIds.MessageBox.CANCEL
					: hasButton(dialog, AutomationIds.MessageBox.YES) && hasButton(dialog, AutomationIds.MessageBox.NO) ? AutomationIds.MessageBox.NO : null;
			}

			string id = dialog.Properties.AutomationId.ValueOrDefault;

			switch (id)
			{
				case AutomationIds.SaveDataSaveDialog.ID:
					return confirm ? AutomationIds.SaveDataSaveDialog.SAVE : AutomationIds.SaveDataSaveDialog.CANCEL;
				case AutomationIds.SaveDataLoadDialog.ID:
					return confirm ? AutomationIds.SaveDataLoadDialog.LOAD : "btnCancel";
				case AutomationIds.ShikigamiRegisterForm.ID:
					return confirm ? AutomationIds.ShikigamiRegisterForm.REGISTER : "btnCancel";
				case AutomationIds.ShikigamiRecoveryDialog.ID:
					return confirm ? AutomationIds.ShikigamiRecoveryDialog.RECOVERY : "btnCancel";
				case AutomationIds.SnapshotCompareFileSelectDialog.ID:
					return confirm ? AutomationIds.SnapshotCompareFileSelectDialog.COMPARE : "btnCancel";
				default:
					return null;
			}
		}

		private bool hasButton(Window dialog, string automationId)
		{
			return dialog.FindFirstDescendant(cf => cf.ByAutomationId(automationId).And(cf.ByControlType(ControlType.Button))) != null;
		}

		public void Close(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			string stage = "Initialize";
			string cacheSource = "Unknown";
			int buttonCount = -1;
			int buttonIndex = -1;

			try
			{
				stage = "GetActiveDialog";

				bool hasCachedDialog = session.LastCheckedDialog != null;
				cacheSource = hasCachedDialog ? "Cached" : "Discovered";

				Window dialog = session.LastCheckedDialog ?? GetActiveDialog(session);

				stage = "GetDialogButtons";

				AutomationElement[] buttons = session.LastCheckedDialogButtons;

				if (buttons == null)
				{
					if (!tryInspectDialog(dialog, out buttons, out AutomationElement[] texts))
					{
						throw new InvalidOperationException("Failed to inspect dialog buttons.");
					}
				}

				buttonCount = buttons.Length;

				stage = "FindOkButton";

				AutomationElement button = null;

				for (int i = 0; i < buttons.Length; i++)
				{
					buttonIndex = i;

					string name = buttons[i].Properties.Name.ValueOrDefault;

					if (string.Equals(name, "OK", StringComparison.OrdinalIgnoreCase))
					{
						button = buttons[i];
						break;
					}
				}

				if (button == null)
				{
					stage = "FindInvokableButton";

					for (int i = 0; i < buttons.Length; i++)
					{
						buttonIndex = i;

						if (buttons[i].Patterns.Invoke.IsSupported)
						{
							button = buttons[i];
							break;
						}
					}
				}

				if (button == null)
				{
					throw new InvalidOperationException("Dialog button was not found.");
				}

				stage = "GetWindowHandle";

				IntPtr windowHandle = dialog.Properties.NativeWindowHandle.ValueOrDefault;

				stage = "ClickButton";

				mButtonOperator.Click(button);

				stage = "WaitForWindowClosed";

				mWindowWaiter.WaitForWindowClosed(windowHandle);

				stage = "ClearDialogCache";

				session.LastCheckedDialog = null;
				session.LastCheckedDialogButtons = null;
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException(
					$"DialogOperator.Close failed at stage '{stage}': " +
					$"CacheSource={cacheSource}, " +
					$"ButtonCount={buttonCount}, " +
					$"ButtonIndex={buttonIndex}. " +
					$"{ex.Message}",
					ex);
			}
		}

		private bool waitForMessageBox(GuiSession session, IntPtr mainWindowHandle, string expectedMessage, out Window dialog, out AutomationElement[] buttons)
		{
			dialog = null;
			buttons = null;

			Stopwatch stopwatch = Stopwatch.StartNew();

			do
			{
				if (tryFindMessageBox(session, mainWindowHandle, expectedMessage, out dialog, out buttons))
				{
					return true;
				}

				int remainingMs = MESSAGE_BOX_TIMEOUT_MS - (int)stopwatch.ElapsedMilliseconds;

				if (remainingMs <= 0)
				{
					break;
				}

				Thread.Sleep(Math.Min(MESSAGE_BOX_INTERVAL_MS, remainingMs));
			}
			while (stopwatch.ElapsedMilliseconds < MESSAGE_BOX_TIMEOUT_MS);

			return false;
		}

		private bool tryFindMessageBox(GuiSession session, IntPtr mainWindowHandle, string expectedMessage, out Window dialog, out AutomationElement[] buttons)
		{
			dialog = null;
			buttons = null;

			uint processId = (uint)session.Application.ProcessId;
			Window foundDialog = null;
			AutomationElement[] foundButtons = null;

			EnumWindows((windowHandle, lParam) =>
			{
				GetWindowThreadProcessId(windowHandle, out uint windowProcessId);

				if (windowProcessId != processId || windowHandle == mainWindowHandle || !isVisible(windowHandle))
				{
					return true;
				}

				if (!isMessageBox(windowHandle))
				{
					return true;
				}

				try
				{
					AutomationElement element = session.Automation.FromHandle(windowHandle);

					if (element == null || !tryInspectDialog(element, out AutomationElement[] candidateButtons, out AutomationElement[] texts))
					{
						return true;
					}

					if (candidateButtons.Length == 0)
					{
						return true;
					}

					bool containsExpectedMessage = texts.Any(text =>
					{
						string name = text.Properties.Name.ValueOrDefault;

						return !string.IsNullOrWhiteSpace(name) && name.Contains(expectedMessage);
					});

					if (!containsExpectedMessage)
					{
						return true;
					}

					Window candidateDialog = element.AsWindow();

					if (candidateDialog == null)
					{
						return true;
					}

					foundDialog = candidateDialog;
					foundButtons = candidateButtons;

					return false;
				}
				catch (PropertyNotSupportedException)
				{
					return true;
				}
				catch (ElementNotAvailableException)
				{
					return true;
				}
				catch (System.Windows.Automation.ElementNotAvailableException)
				{
					return true;
				}
				catch (COMException)
				{
					return true;
				}
			}, IntPtr.Zero);

			dialog = foundDialog;
			buttons = foundButtons;

			return dialog != null;
		}

		private bool isMessageBox(IntPtr windowHandle)
		{
			var className = new System.Text.StringBuilder(256);

			if (GetClassName(windowHandle, className, className.Capacity) == 0)
			{
				return false;
			}

			return string.Equals(className.ToString(), "#32770", StringComparison.Ordinal);
		}

		private bool isVisible(IntPtr windowHandle)
		{
			return windowHandle != IntPtr.Zero && IsWindowVisible(windowHandle);
		}

		private bool tryInspectDialog(AutomationElement element, out AutomationElement[] buttons, out AutomationElement[] texts)
		{
			buttons = null;
			texts = null;

			try
			{
				AutomationElement[] dialogElements = element.FindAllDescendants(cf => cf.ByControlType(ControlType.Button).Or(cf.ByControlType(ControlType.Text)));

				var buttonElements = new List<AutomationElement>();
				var textElements = new List<AutomationElement>();

				foreach (AutomationElement dialogElement in dialogElements)
				{
					if (!belongsToWindow(element, dialogElement))
					{
						continue;
					}

					ControlType controlType = dialogElement.Properties.ControlType.ValueOrDefault;

					if (controlType == ControlType.Button)
					{
						buttonElements.Add(dialogElement);
					}
					else if (controlType == ControlType.Text)
					{
						textElements.Add(dialogElement);
					}
				}

				buttons = buttonElements.ToArray();
				texts = textElements.ToArray();

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
		}

		private bool belongsToWindow(AutomationElement windowElement, AutomationElement element)
		{
			IntPtr windowHandle = windowElement.Properties.NativeWindowHandle.ValueOrDefault;

			if (windowHandle == IntPtr.Zero)
			{
				return false;
			}

			AutomationElement parent = element.Parent;

			while (parent != null)
			{
				if (parent.Properties.ControlType.ValueOrDefault == ControlType.Window)
				{
					return parent.Properties.NativeWindowHandle.ValueOrDefault == windowHandle;
				}

				parent = parent.Parent;
			}

			return false;
		}
	}
}
