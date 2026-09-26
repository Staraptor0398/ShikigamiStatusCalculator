using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
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

		private readonly ButtonOperator mButtonOperator;
		private readonly GuiOperator mGuiOperator;

		private readonly WindowWaiter mWindowWaiter;

		private Window mLastCheckedDialog;
		private AutomationElement[] mLastCheckedDialogButtons;

		public DialogOperator()
		{
			mButtonOperator = new ButtonOperator();
			mGuiOperator = new GuiOperator();

			mWindowWaiter = new WindowWaiter();
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

			Window dialog = mWindowWaiter.WaitForProcessWindow(session, mainWindow, element =>
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

			Window dialog = mWindowWaiter.FindProcessWindow(session, mainWindow, element =>
			{
				IntPtr windowHandle = element.Properties.NativeWindowHandle.ValueOrDefault;

				if (windowHandle == mainWindowHandle || !isVisible(windowHandle))
				{
					return false;
				}

				if (!tryInspectDialog(element, out AutomationElement[] buttons, out AutomationElement[] texts))
				{
					return false;
				}

				return buttons.Length > 0 && texts.Length > 0;
			});

			return dialog != null;
		}

		public string GetMessage(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			Window dialog = mLastCheckedDialog ?? GetActiveDialog(session);

			try
			{
				return dialog.FindAllDescendants(cf => cf.ByControlType(ControlType.Text)).Select(element => element.Properties.Name.ValueOrDefault).FirstOrDefault(text => !string.IsNullOrWhiteSpace(text));
			}
			catch (COMException)
			{
				return null;
			}
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

			if (mainWindow != null)
			{
				mLastCheckedDialog = mWindowWaiter.WaitForProcessWindow(session, mainWindow, predicate);
			}
			else
			{
				mLastCheckedDialog = mWindowWaiter.WaitForProcessWindow(session, predicate);
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
				try
				{
					buttons = dialog.FindAllDescendants(cf => cf.ByControlType(ControlType.Button));
				}
				catch (COMException ex)
				{
					throw new InvalidOperationException("Failed to inspect dialog buttons.", ex);
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
			catch (COMException)
			{
				return false;
			}
		}
	}
}
