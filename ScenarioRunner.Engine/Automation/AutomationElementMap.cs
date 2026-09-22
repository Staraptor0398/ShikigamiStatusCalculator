using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScenarioRunner.Automation
{
	public class AutomationElementMap
	{
		private readonly Dictionary<string, List<AutomationElement>> mElements;

		public AutomationElementMap(AutomationElement parent)
		{
			if (parent == null)
			{
				throw new ArgumentNullException(nameof(parent));
			}

			mElements = new Dictionary<string, List<AutomationElement>>(StringComparer.Ordinal);

			AutomationElement[] elements = parent.FindAllDescendants();

			foreach (AutomationElement element in elements)
			{
				string automationId = element.Properties.AutomationId.ValueOrDefault;

				if (string.IsNullOrWhiteSpace(automationId))
				{
					continue;
				}

				if (!mElements.TryGetValue(automationId, out List<AutomationElement> matchedElements))
				{
					matchedElements = new List<AutomationElement>();
					mElements.Add(automationId, matchedElements);
				}

				matchedElements.Add(element);
			}
		}

		public AutomationElement Get(string automationId, ControlType controlType)
		{
			if (string.IsNullOrWhiteSpace(automationId))
			{
				throw new ArgumentException("AutomationId is empty.", nameof(automationId));
			}

			if (!mElements.TryGetValue(automationId, out List<AutomationElement> matchedElements))
			{
				throw new InvalidOperationException($"Automation element was not found: {automationId}");
			}

			AutomationElement element = matchedElements.FirstOrDefault(candidate => candidate.Properties.ControlType.ValueOrDefault == controlType);

			if (element == null)
			{
				throw new InvalidOperationException($"Automation element was not found: {automationId}, ControlType={controlType}");
			}

			return element;
		}

		public AutomationElement Get(string automationId)
		{
			if (string.IsNullOrWhiteSpace(automationId))
			{
				throw new ArgumentException("AutomationId is empty.", nameof(automationId));
			}

			if (!mElements.TryGetValue(automationId, out List<AutomationElement> matchedElements))
			{
				throw new InvalidOperationException($"Automation element was not found: {automationId}");
			}

			return matchedElements[0];
		}
	}
}
