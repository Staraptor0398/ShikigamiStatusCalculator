using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using System;
using System.Diagnostics;

namespace ScenarioRunner.Automation.Operator
{
	public class ButtonOperator
	{
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

			Stopwatch stopwatch = Stopwatch.StartNew();

			logButtonInvokePerf("Invoke START", element, stopwatch);

			button.Invoke();

			logButtonInvokePerf("Invoke END", element, stopwatch);
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

		private static void logButtonInvokePerf(string phase, AutomationElement element, Stopwatch stopwatch)
		{
			Console.WriteLine(
				$"[ButtonInvokePerf] {phase} | " +
				$"AutomationId={element.Properties.AutomationId.ValueOrDefault} | " +
				$"Elapsed={stopwatch.Elapsed.TotalMilliseconds:F1} ms");
		}
	}
}
