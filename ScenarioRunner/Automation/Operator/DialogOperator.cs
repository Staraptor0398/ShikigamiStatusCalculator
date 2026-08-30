using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using ScenarioRunner.Automation.Waiter;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace ScenarioRunner.Automation.Operator
{
	public class DialogOperator
	{
		private readonly ButtonOperator mButtonOperator;
		private readonly GuiOperator mGuiOperator;

		private readonly WindowWaiter mWindowWaiter;

		private Window mLastCheckedDialog;

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

			int processId = session.Application.ProcessId;
			Window mainWindow = mGuiOperator.GetMainWindow(session);
			IntPtr mainWindowHandle = mainWindow.Properties.NativeWindowHandle.Value;

			return mWindowWaiter.WaitForWindow(session, element => element.Properties.ProcessId.ValueOrDefault == processId && element.Properties.NativeWindowHandle.ValueOrDefault != mainWindowHandle && isDialog(element));
		}

		public bool Exists(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			int processId = session.Application.ProcessId;
			Window mainWindow = mGuiOperator.GetMainWindow(session);
			IntPtr mainWindowHandle = mainWindow.Properties.NativeWindowHandle.Value;

			return mWindowWaiter.Exists(session, element => element.Properties.ProcessId.ValueOrDefault == processId && element.Properties.NativeWindowHandle.ValueOrDefault != mainWindowHandle);
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
				return dialog
					.FindAllDescendants(cf => cf.ByControlType(ControlType.Text))
					.Select(element => element.Properties.Name.ValueOrDefault)
					.FirstOrDefault(text => !string.IsNullOrWhiteSpace(text));
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

			int processId = session.Application.ProcessId;

			mLastCheckedDialog = mWindowWaiter.WaitForWindow(session, element => element.Properties.ProcessId.ValueOrDefault == processId && containsMessage(element, expectedMessage));
		}

		public void Close(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			Window dialog = mLastCheckedDialog ?? GetActiveDialog(session);

			AutomationElement[] buttons;

			try
			{
				buttons = dialog.FindAllDescendants(cf => cf.ByControlType(ControlType.Button));
			}
			catch (COMException ex)
			{
				throw new InvalidOperationException("Failed to inspect dialog buttons.", ex);
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

			mLastCheckedDialog = null;
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

		private bool containsMessage(AutomationElement element, string expectedMessage)
		{
			try
			{
				AutomationElement[] textElements = element.FindAllDescendants(cf => cf.ByControlType(ControlType.Text));

				return textElements.Any(
					textElement =>
					{
						string text = textElement.Properties.Name.ValueOrDefault;

						return
							!string.IsNullOrWhiteSpace(text) && text.Contains(expectedMessage);
					});
			}
			catch (COMException)
			{
				return false;
			}
		}
	}
}
