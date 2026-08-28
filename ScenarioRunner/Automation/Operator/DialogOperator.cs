using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using System;
using System.Linq;

namespace ScenarioRunner.Automation.Operator
{
	public class DialogOperator
	{
		private readonly WindowOperator mWindowOperator;
		private readonly ButtonOperator mButtonOperator;
		private readonly GuiOperator mGuiOperator;

		private Window mLastCheckedDialog;

		public DialogOperator()
		{
			mWindowOperator = new WindowOperator();
			mButtonOperator = new ButtonOperator();
			mGuiOperator = new GuiOperator();
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

			return mWindowOperator.WaitForWindow(session, element => element.Properties.ProcessId.Value == processId && element.Properties.NativeWindowHandle.Value != mainWindowHandle && isDialog(element));
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

			return mWindowOperator.Exists(session, element => element.Properties.ProcessId.Value == processId && element.Properties.NativeWindowHandle.Value != mainWindowHandle);
		}

		public string GetMessage(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			Window dialog = mLastCheckedDialog ?? GetActiveDialog(session);

			return dialog.FindAllDescendants(cf => cf.ByControlType(ControlType.Text)).Select(element => element.Name).FirstOrDefault(text => !string.IsNullOrWhiteSpace(text));
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

			mLastCheckedDialog = mWindowOperator.WaitForWindow(session, element => element.Properties.ProcessId.Value == processId && containsMessage(element, expectedMessage));
		}

		public void Close(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			Window dialog = mLastCheckedDialog ?? GetActiveDialog(session);

			AutomationElement[] buttons = dialog.FindAllDescendants(cf => cf.ByControlType(ControlType.Button));

			AutomationElement button = buttons.FirstOrDefault(element => string.Equals(element.Name, "OK", StringComparison.OrdinalIgnoreCase));

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
			AutomationElement button = element.FindFirstDescendant(cf => cf.ByControlType(ControlType.Button));

			AutomationElement text = element.FindFirstDescendant(cf => cf.ByControlType(ControlType.Text));

			return button != null && text != null;
		}

		private bool containsMessage(AutomationElement element, string expectedMessage)
		{
			AutomationElement[] textElements = element.FindAllDescendants(cf => cf.ByControlType(ControlType.Text));

			return textElements.Any(textElement => !string.IsNullOrWhiteSpace(textElement.Name) && textElement.Name.Contains(expectedMessage));
		}
	}
}
