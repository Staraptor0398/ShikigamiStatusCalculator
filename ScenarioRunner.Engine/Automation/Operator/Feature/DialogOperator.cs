using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Exceptions;
using ScenarioRunner.Automation.Definition;
using ScenarioRunner.Automation.Waiter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace ScenarioRunner.Automation.Operator.Feature
{
	public class DialogOperator
	{
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool IsWindowVisible(IntPtr hWnd);

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

		private Window mLastCheckedDialog;
		private AutomationElement[] mLastCheckedDialogButtons;

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

			mLastCheckedDialog = null;
			mLastCheckedDialogButtons = null;

			Window mainWindow = mGuiOperator.GetMainWindow(session);
			IntPtr mainWindowHandle = mainWindow.Properties.NativeWindowHandle.Value;

			AutomationElement[] buttons = null;

			Window dialog = mProcessWindowWaiter.WaitForProcessWindow(session, mainWindow, element =>
			{
				IntPtr windowHandle = element.Properties.NativeWindowHandle.ValueOrDefault;

				if (windowHandle == mainWindowHandle || !isVisible(windowHandle))
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
			});

			mLastCheckedDialog = dialog;
			mLastCheckedDialogButtons = buttons;

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

			Window dialog = mLastCheckedDialog ?? GetActiveDialog(session);

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

			mLastCheckedDialog = null;
			mLastCheckedDialogButtons = null;

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

				mLastCheckedDialogButtons = buttons;

				return true;
			};

			if (tryFindMessageBox(session, mainWindowHandle, expectedMessage, out Window messageBox, out AutomationElement[] messageBoxButtons))
			{
				mLastCheckedDialog = messageBox;
				mLastCheckedDialogButtons = messageBoxButtons;
				return;
			}

			if (mainWindow != null)
			{
				mLastCheckedDialog = mProcessWindowWaiter.WaitForProcessWindow(session, mainWindow, predicate);
			}
			else
			{
				mLastCheckedDialog = mProcessWindowWaiter.WaitForProcessWindow(session, predicate);
			}
		}

		public void Close(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			Window dialog = mLastCheckedDialog ?? GetActiveDialog(session);

			AutomationElement[] buttons = mLastCheckedDialogButtons;

			if (buttons == null)
			{
				if (!tryInspectDialog(dialog, out buttons, out AutomationElement[] texts))
				{
					throw new InvalidOperationException("Failed to inspect dialog buttons.");
				}
			}

			AutomationElement button = buttons.FirstOrDefault(element => string.Equals(element.Properties.Name.ValueOrDefault, "OK", StringComparison.OrdinalIgnoreCase));

			if (button == null)
			{
				button = buttons.FirstOrDefault(element => element.Patterns.Invoke.IsSupported);
			}

			if (button == null)
			{
				throw new InvalidOperationException("Dialog button was not found.");
			}

			mButtonOperator.Click(button);

			mWindowWaiter.WaitForWindowClosed(dialog);

			mLastCheckedDialog = null;
			mLastCheckedDialogButtons = null;
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
