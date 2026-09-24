using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using System;
using System.Runtime.InteropServices;

namespace ScenarioRunner.Automation.Operator
{
	public class ButtonOperator
	{
		private const uint BM_CLICK = 0x00F5;

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

		public void Click(AutomationElement parent, string automationId)
		{
			if (parent == null)
			{
				throw new ArgumentNullException(nameof(parent));
			}

			var buttonElement = parent.FindFirstDescendant(cf => cf.ByAutomationId(automationId).And(cf.ByControlType(ControlType.Button)));

			if (buttonElement == null)
			{
				throw new InvalidOperationException($"Button was not found: {automationId}");
			}

			buttonElement.AsButton().Invoke();
		}

		public void Click(AutomationElement element)
		{
			if (element == null)
			{
				throw new ArgumentNullException(nameof(element));
			}

			Button button = element.AsButton();
			button.Invoke();
		}

		public void PostClick(AutomationElement element)
		{
			if (element == null)
			{
				throw new ArgumentNullException(nameof(element));
			}

			IntPtr windowHandle = element.Properties.NativeWindowHandle.ValueOrDefault;

			if (windowHandle == IntPtr.Zero)
			{
				throw new InvalidOperationException("Button has no native window handle.");
			}

			if (!PostMessage(windowHandle, BM_CLICK, IntPtr.Zero, IntPtr.Zero))
			{
				throw new InvalidOperationException("Button click message could not be posted.");
			}
		}

		public bool IsEnabled(AutomationElement parent, string automationId)
		{
			if (parent == null)
			{
				throw new ArgumentNullException(nameof(parent));
			}

			var buttonElement = parent.FindFirstDescendant(cf => cf.ByAutomationId(automationId).And(cf.ByControlType(ControlType.Button)));

			if (buttonElement == null)
			{
				throw new InvalidOperationException($"Button was not found: {automationId}");
			}

			return buttonElement.Properties.IsEnabled.ValueOrDefault;
		}
	}
}
