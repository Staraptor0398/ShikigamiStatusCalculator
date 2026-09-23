using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
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

			Window mainWindow = mGuiOperator.GetMainWindow(session);
			IntPtr mainWindowHandle = mainWindow.Properties.NativeWindowHandle.Value;

			return mWindowWaiter.WaitForProcessWindow(session, element => element.Properties.NativeWindowHandle.ValueOrDefault != mainWindowHandle && isVisible(element) && isDialog(element));
		}

		public bool Exists(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			Window mainWindow = mGuiOperator.GetMainWindow(session);
			IntPtr mainWindowHandle = mainWindow.Properties.NativeWindowHandle.Value;

			Window dialog = mWindowWaiter.FindProcessWindow(session, element => element.Properties.NativeWindowHandle.ValueOrDefault != mainWindowHandle && isVisible(element) && isDialog(element));

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

			Window dialog =
				mLastCheckedDialog ?? GetActiveDialog(session);

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

			Window mainWindow = mGuiOperator.GetMainWindow(session);
			IntPtr mainWindowHandle = mainWindow.Properties.NativeWindowHandle.Value;

			mLastCheckedDialog = mWindowWaiter.WaitForProcessWindow(
				session,
				element =>
				{
					if (element.Properties.NativeWindowHandle.ValueOrDefault == mainWindowHandle || !isVisible(element))
					{
						return false;
					}

					if (!containsMessage(element, expectedMessage, out AutomationElement[] buttons))
					{
						return false;
					}

					mLastCheckedDialogButtons = buttons;

					return true;
				}
			);
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

		private bool isDialog(AutomationElement element)
		{
			try
			{
				AutomationElement button = element.FindFirstDescendant(cf => cf.ByControlType(ControlType.Button));

				AutomationElement text = element.FindFirstDescendant(cf => cf.ByControlType(ControlType.Text));

				return button != null && text != null;
			}
			catch (COMException)
			{
				return false;
			}
		}

		private bool containsMessage(AutomationElement element, string expectedMessage, out AutomationElement[] buttons)
		{
			buttons = null;

			try
			{
				AutomationElement[] descendants = element.FindAllDescendants();

				bool containsExpectedMessage = descendants.Any(descendant => descendant.Properties.ControlType.ValueOrDefault == ControlType.Text && !string.IsNullOrWhiteSpace(descendant.Properties.Name.ValueOrDefault) && descendant.Properties.Name.ValueOrDefault.Contains(expectedMessage));

				if (!containsExpectedMessage)
				{
					return false;
				}

				buttons = descendants.Where(descendant => descendant.Properties.ControlType.ValueOrDefault == ControlType.Button).ToArray();

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
