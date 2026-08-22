using FlaUI.Core.AutomationElements;
using System;

namespace ScenarioRunner.Automation.Operator
{
	public class TextBoxOperator
	{
		public string GetText(AutomationElement parent, string automationId)
		{
			if (parent == null)
			{
				throw new ArgumentNullException(nameof(parent));
			}

			var element = parent.FindFirstDescendant(cf => cf.ByAutomationId(automationId));

			if (element == null)
			{
				throw new InvalidOperationException($"TextBox was not found: {automationId}");
			}

			return element.AsTextBox().Text;
		}
	}
}
