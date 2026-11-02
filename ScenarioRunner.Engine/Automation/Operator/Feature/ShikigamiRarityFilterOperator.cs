using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using ScenarioRunner.Automation.Definition;
using ScenarioRunner.Execution;
using System;
using System.Collections.Generic;
using System.IO;

namespace ScenarioRunner.Automation.Operator.Feature
{
	public class ShikigamiRarityFilterOperator
	{
		private const string SHIKIGAMI_DATA_RELATIVE_PATH = @"Data\ShikigamiData.csv";
		private const string CSV_HEADER = "レア度,式神名,攻撃力,HP,防御力,素早さ,会心率,会心DMG,効果命中,効果抵抗";
		private const string RARITY_ALL = "すべて";

		private readonly ShikigamiOperator mShikigamiOperator;
		private readonly ShikigamiDataFileOperator mFileOperator;
		private readonly ComboBoxOperator mComboBoxOperator;
		private readonly GuiOperator mGuiOperator;

		public ShikigamiRarityFilterOperator()
		{
			mShikigamiOperator = new ShikigamiOperator();
			mFileOperator = new ShikigamiDataFileOperator();
			mComboBoxOperator = new ComboBoxOperator();
			mGuiOperator = new GuiOperator();
		}

		public void Select(GuiSession session, string rarity)
		{
			AutomationElement element = getRarityFilter(session);

			mComboBoxOperator.SelectItem(element, rarity);
		}

		public void CheckRarity(GuiSession session, string expectedRarity)
		{
			AutomationElement element = getRarityFilter(session);
			string actualRarity = mComboBoxOperator.GetValue(element);

			if (!string.Equals(expectedRarity, actualRarity, StringComparison.Ordinal))
			{
				throw new InvalidOperationException($"Rarity filter mismatch: Expected={expectedRarity}, Actual={actualRarity ?? "<null>"}");
			}
		}

		public void CheckShikigamiList(ScenarioExecutionContext context, string rarity)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			IReadOnlyList<string> expected = readExpectedShikigamiNames(context, rarity);

			AutomationElement element = mGuiOperator.GetMainElement(context.GuiSession, AutomationIds.MainForm.SHIKIGAMI, ControlType.ComboBox);

			IReadOnlyList<string> actual = mComboBoxOperator.GetItems(element);

			Dictionary<string, int> expectedCounts = countNames(expected);
			Dictionary<string, int> actualCounts = countNames(actual);

			var missing = new List<string>();
			var extra = new List<string>();

			foreach (KeyValuePair<string, int> entry in expectedCounts)
			{
				actualCounts.TryGetValue(entry.Key, out int actualCount);

				for (int i = actualCount; i < entry.Value; i++)
				{
					missing.Add(entry.Key);
				}
			}

			foreach (KeyValuePair<string, int> entry in actualCounts)
			{
				expectedCounts.TryGetValue(entry.Key, out int expectedCount);

				for (int i = expectedCount; i < entry.Value; i++)
				{
					extra.Add(entry.Key);
				}
			}

			if (missing.Count != 0 || extra.Count != 0)
			{
				throw new InvalidOperationException(
					$"Shikigami list mismatch: Rarity={rarity}, " +
					$"ExpectedCount={expected.Count}, ActualCount={actual.Count}, " +
					$"Missing=[{string.Join(", ", missing)}], " +
					$"Extra=[{string.Join(", ", extra)}]");
			}
		}

		public void CheckShikigamiSelection(GuiSession session, string expectedName, bool expectNone)
		{
			mShikigamiOperator.CheckSelection(session, expectedName, expectNone);
		}

		private AutomationElement getRarityFilter(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			return mGuiOperator.GetMainElement(session, AutomationIds.MainForm.RARITY_FILTER, ControlType.ComboBox);
		}

		private IReadOnlyList<string> readExpectedShikigamiNames(ScenarioExecutionContext context, string rarity)
		{
			string directory = Path.GetDirectoryName(context.GuiExecutablePath);

			if (string.IsNullOrWhiteSpace(directory))
			{
				throw new InvalidOperationException("Gui executable directory could not be resolved.");
			}

			string filePath = Path.Combine(directory, SHIKIGAMI_DATA_RELATIVE_PATH);
			string[] lines = mFileOperator.ReadAllLines(filePath);

			if (lines.Length == 0 || !string.Equals(lines[0].TrimStart('\uFEFF'), CSV_HEADER, StringComparison.Ordinal))
			{
				throw new InvalidOperationException("ShikigamiData.csv header is invalid.");
			}

			var names = new List<string>();

			for (int i = 1; i < lines.Length; i++)
			{
				string[] columns = lines[i].Split(',');

				if (columns.Length != 10 || !isValidRarity(columns[0]) || string.IsNullOrWhiteSpace(columns[1]))
				{
					throw new InvalidOperationException($"Invalid ShikigamiData.csv row: Line={i + 1}");
				}

				if (rarity == RARITY_ALL || string.Equals(columns[0], rarity, StringComparison.Ordinal))
				{
					names.Add(columns[1]);
				}
			}

			return names;
		}

		private bool isValidRarity(string rarity)
		{
			switch (rarity)
			{
				case "UR":
				case "SP":
				case "SSR":
				case "SR":
					return true;

				default:
					return false;
			}
		}

		private Dictionary<string, int> countNames(IEnumerable<string> names)
		{
			var counts = new Dictionary<string, int>(StringComparer.Ordinal);

			foreach (string name in names)
			{
				counts.TryGetValue(name, out int count);
				counts[name] = count + 1;
			}

			return counts;
		}
	}
}
