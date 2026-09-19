using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using System;

namespace ScenarioRunner.Automation.Operator
{
	public class DataGridViewOperator
	{
		public string GetCellValue(AutomationElement parent, string automationId, string rowName, int columnIndex)
		{
			if (parent == null)
			{
				throw new ArgumentNullException(nameof(parent));
			}

			if (string.IsNullOrWhiteSpace(automationId))
			{
				throw new ArgumentException("AutomationId is empty.", nameof(automationId));
			}

			if (string.IsNullOrWhiteSpace(rowName))
			{
				throw new ArgumentException("Row name is empty.", nameof(rowName));
			}

			AutomationElement dataGridViewElement = parent.FindFirstDescendant(cf => cf.ByAutomationId(automationId).And(cf.ByControlType(ControlType.Table)));

			if (dataGridViewElement == null)
			{
				throw new InvalidOperationException($"DataGridView was not found: {automationId}");
			}

			DataGridView dataGridView = dataGridViewElement.AsDataGridView();

			foreach (DataGridViewRow row in dataGridView.Rows)
			{
				DataGridViewCell[] cells = row.Cells;

				if (cells.Length == 0)
				{
					continue;
				}

				if (cells[0].Value != rowName)
				{
					continue;
				}

				if (columnIndex < 0 || columnIndex >= cells.Length)
				{
					throw new InvalidOperationException($"Column index is out of range: {columnIndex}");
				}

				return cells[columnIndex].Value;
			}

			throw new InvalidOperationException($"DataGridView row was not found: {rowName}");
		}
	}
}
