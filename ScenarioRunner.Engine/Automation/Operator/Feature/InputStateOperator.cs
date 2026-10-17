using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using ScenarioRunner.Automation.Definition;
using ScenarioRunner.Execution;
using System;
using System.Collections.Generic;

namespace ScenarioRunner.Automation.Operator.Feature
{
	public class InputStateOperator
	{
		private readonly ComboBoxOperator mComboBoxOperator;
		private readonly TextBoxOperator mTextBoxOperator;
		private readonly GuiOperator mGuiOperator;

		public InputStateOperator()
		{
			mComboBoxOperator = new ComboBoxOperator();
			mTextBoxOperator = new TextBoxOperator();
			mGuiOperator = new GuiOperator();
		}

		public void Capture(ScenarioExecutonContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			// 全項目を正常に読み取れた場合のみ基準値を上書きする。
			Dictionary<string, string> snapshot = readInputState(context.GuiSession);

			context.CapturedInputState = snapshot;
		}

		public void CheckUnchanged(ScenarioExecutonContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			if (context.CapturedInputState == null)
			{
				throw new InvalidOperationException("Input state has not been captured.");
			}

			Dictionary<string, string> actual = readInputState(context.GuiSession);

			foreach (KeyValuePair<string, string> entry in context.CapturedInputState)
			{
				if (!actual.TryGetValue(entry.Key, out string value))
				{
					throw new InvalidOperationException($"Input state item was not found: {entry.Key}");
				}

				if (!string.Equals(entry.Value, value, StringComparison.Ordinal))
				{
					throw new InvalidOperationException($"Input state has changed: {entry.Key}, Expected={escape(entry.Value)}, Actual={escape(value)}");
				}
			}
		}

		private Dictionary<string, string> readInputState(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			AutomationElementMap elementMap = mGuiOperator.GetMainElementMap(session);

			var snapshot = new Dictionary<string, string>(StringComparer.Ordinal);

			readComboBox(elementMap, snapshot, AutomationIds.MainForm.SHIKIGAMI);

			for (int mitamaSlot = 1; mitamaSlot <= 6; mitamaSlot++)
			{
				readComboBox(elementMap, snapshot, AutomationIds.MainForm.MainStat(mitamaSlot));
				readTextBox(elementMap, snapshot, AutomationIds.MainForm.MainStatValue(mitamaSlot));

				for (int subSlot = 1; subSlot <= 4; subSlot++)
				{
					readComboBox(elementMap, snapshot, AutomationIds.MainForm.SubStat(mitamaSlot, subSlot));
					readTextBox(elementMap, snapshot, AutomationIds.MainForm.SubStatValue(mitamaSlot, subSlot));
				}
			}

			for (int slot = 1; slot <= 3; slot++)
			{
				readComboBox(elementMap, snapshot, AutomationIds.MainForm.SetEffect(slot));
			}

			for (int slot = 1; slot <= 6; slot++)
			{
				readComboBox(elementMap, snapshot, AutomationIds.MainForm.UniqueEffect(slot));
			}

			if (snapshot.Count != 70)
			{
				throw new InvalidOperationException($"Unexpected input state item count: {snapshot.Count}");
			}

			return snapshot;
		}

		private void readComboBox(AutomationElementMap elementMap, Dictionary<string, string> snapshot, string automationId)
		{
			AutomationElement element = elementMap.Get(automationId, ControlType.ComboBox);

			string value = mComboBoxOperator.GetValue(element);

			if (value == null)
			{
				throw new InvalidOperationException($"ComboBox value could not be read: {automationId}");
			}

			snapshot.Add(automationId, value);
		}

		private void readTextBox(AutomationElementMap elementMap, Dictionary<string, string> snapshot, string automationId)
		{
			AutomationElement element = elementMap.Get(automationId);

			string value = mTextBoxOperator.GetText(element);

			if (value == null)
			{
				throw new InvalidOperationException($"TextBox value could not be read: {automationId}");
			}

			snapshot.Add(automationId, value);
		}

		private static string escape(string value)
		{
			if (value == null)
			{
				return "<null>";
			}

			return "\"" + value.Replace("\\", "\\\\").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t").Replace("\"", "\\\"") + "\"";
		}
	}
}
