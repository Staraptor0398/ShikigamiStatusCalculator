using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using System;

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

			element.AsButton().Invoke();
		}
	}
}
