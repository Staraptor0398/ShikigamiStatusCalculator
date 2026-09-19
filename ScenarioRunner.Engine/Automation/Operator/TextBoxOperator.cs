using FlaUI.Core.AutomationElements;
using System;

namespace ScenarioRunner.Automation.Operator
{
	public class TextBoxOperator
	{
		public void SetText(AutomationElement parent, string automationId, string text)
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

			element.AsTextBox().Text = text;
		}

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
