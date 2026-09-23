using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using System;
using System.Collections.Generic;

namespace ScenarioRunner.Automation.Operator
{
	public class DataGridViewOperator
	{
		public string GetCellValue(AutomationElement parent, string automationId, string rowName, int columnIndex)
		{
			IReadOnlyDictionary<string, string[]> rowValues = GetRowValues(parent, automationId);

			return GetCellValue(rowValues, rowName, columnIndex);
		}

		public string GetCellValue(IReadOnlyDictionary<string, string[]> rowValues, string rowName, int columnIndex)
		{
			if (rowValues == null)
			{
				throw new ArgumentNullException(nameof(rowValues));
			}

			if (string.IsNullOrWhiteSpace(rowName))
			{
				throw new ArgumentException("Row name is empty.", nameof(rowName));
			}

			if (!rowValues.TryGetValue(rowName, out string[] cells))
			{
				throw new InvalidOperationException($"DataGridView row was not found: {rowName}");
			}

			if (columnIndex < 0 || columnIndex >= cells.Length)
			{
				throw new InvalidOperationException($"Column index is out of range: {columnIndex}");
			}

			return cells[columnIndex];
		}

		public IReadOnlyDictionary<string, string[]> GetRowValues(AutomationElement parent, string automationId)
		{
			if (parent == null)
			{
				throw new ArgumentNullException(nameof(parent));
			}

			if (string.IsNullOrWhiteSpace(automationId))
			{
				throw new ArgumentException("AutomationId is empty.", nameof(automationId));
			}

			AutomationElement dataGridViewElement = parent.FindFirstDescendant(cf => cf.ByAutomationId(automationId).And(cf.ByControlType(ControlType.Table)));

			if (dataGridViewElement == null)
			{
				throw new InvalidOperationException($"DataGridView was not found: {automationId}");
			}

			DataGridView dataGridView = dataGridViewElement.AsDataGridView();

			var rowValues = new Dictionary<string, string[]>(StringComparer.Ordinal);

			foreach (DataGridViewRow row in dataGridView.Rows)
			{
				DataGridViewCell[] cells = row.Cells;

				if (cells.Length == 0)
				{
					continue;
				}

				string rowName = cells[0].Value;

				if (string.IsNullOrWhiteSpace(rowName))
				{
					continue;
				}

				if (rowValues.ContainsKey(rowName))
				{
					throw new InvalidOperationException($"Duplicate DataGridView row was found: {rowName}");
				}

				var values = new string[cells.Length];

				for (int i = 0; i < cells.Length; i++)
				{
					values[i] = cells[i].Value;
				}

				rowValues.Add(rowName, values);
			}

			return rowValues;
		}
	}
}
