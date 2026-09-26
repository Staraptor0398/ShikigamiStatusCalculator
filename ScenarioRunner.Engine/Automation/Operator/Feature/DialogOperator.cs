using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using ScenarioRunner.Automation.Definition;
using ScenarioRunner.Automation.Waiter;
using System;
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

		public string LastDetectedDialogInfo { get; private set; }

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
				if (element.Properties.NativeWindowHandle.ValueOrDefault == mainWindowHandle || !isVisible(element))
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
				if (element.Properties.NativeWindowHandle.ValueOrDefault == mainWindowHandle || !isVisible(element))
				{
					return false;
				}

				if (!tryInspectDialog(element, out AutomationElement[] buttons, out AutomationElement[] texts))
				{
					return false;
				}

				return buttons.Length > 0 && texts.Length > 0;
			});

			if (dialog == null)
			{
				LastDetectedDialogInfo = null;
				return false;
			}

			LastDetectedDialogInfo = createDialogInfo(dialog);

			return true;
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

				if ((mainWindowHandle != IntPtr.Zero && windowHandle == mainWindowHandle) || automationId == AutomationIds.MainForm.ID || !isVisible(element))
				{
					return false;
				}

				if (!tryInspectDialog(element, out AutomationElement[] buttons, out AutomationElement[] texts))
				{
					return false;
				}

				bool containsExpectedMessage = texts.Any(text => !string.IsNullOrWhiteSpace(text.Properties.Name.ValueOrDefault) && text.Properties.Name.ValueOrDefault.Contains(expectedMessage));

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

		private bool isVisible(AutomationElement element)
		{
			try
			{
				IntPtr handle = element.Properties.NativeWindowHandle.ValueOrDefault;

				return handle != IntPtr.Zero && IsWindowVisible(handle);
			}
			catch (COMException)
			{
				return false;
			}
		}

		private bool tryInspectDialog(AutomationElement element, out AutomationElement[] buttons, out AutomationElement[] texts)
		{
			buttons = null;
			texts = null;

			try
			{
				AutomationElement[] dialogElements = element.FindAllDescendants(cf => cf.ByControlType(ControlType.Button).Or(cf.ByControlType(ControlType.Text)));

				buttons = dialogElements.Where(dialogElement => dialogElement.Properties.ControlType.ValueOrDefault == ControlType.Button).ToArray();
				texts = dialogElements.Where(dialogElement => dialogElement.Properties.ControlType.ValueOrDefault == ControlType.Text).ToArray();

				return true;
			}
			catch (COMException)
			{
				return false;
			}
		}

		private string createDialogInfo(AutomationElement element)
		{
			try
			{
				return
					$"Name={element.Properties.Name.ValueOrDefault}, " +
					$"AutomationId={element.Properties.AutomationId.ValueOrDefault}, " +
					$"ClassName={element.Properties.ClassName.ValueOrDefault}, " +
					$"ControlType={element.Properties.ControlType.ValueOrDefault}, " +
					$"NativeWindowHandle={element.Properties.NativeWindowHandle.ValueOrDefault}, " +
					$"ProcessId={element.Properties.ProcessId.ValueOrDefault}";
			}
			catch (COMException)
			{
				return "Detected dialog information could not be read.";
			}
		}
	}
}
