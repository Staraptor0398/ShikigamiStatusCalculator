using ShikigamiDataAuditor.Model;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ShikigamiDataAuditor.Access
{
	public class OfficialChangeHistoryCsvReader
	{
		private static readonly IReadOnlyList<string> HEADERS = new[]
		{
			"effective_date", "announcement_date", "rarity", "shikigami", "stat_scope",
			"stat", "old_value", "new_value", "unit", "old_value_known", "current_app_scope",
			"review_priority", "app_action", "source_url", "notes"
		};

		public IReadOnlyList<OfficialStatChange> Read(string filePath)
		{
			CsvDocument document = CsvDocument.Load(filePath, HEADERS);

			List<OfficialStatChange> changes = new List<OfficialStatChange>();

			foreach (IReadOnlyDictionary<string, string> row in document.Rows)
			{
				changes.Add(new OfficialStatChange
				{
					EffectiveDate = parseDate(row["effective_date"]),
					AnnouncementDate = parseDate(row["announcement_date"]),
					Rarity = require(row["rarity"], "rarity"),
					ShikigamiName = require(row["shikigami"], "shikigami"),
					StatScope = parseScope(row["stat_scope"]),
					StatType = StatTypeDefinition.Parse(row["stat"]),
					OldValue = parseNullableDouble(row["old_value"]),
					NewValue = parseDouble(row["new_value"]),
					Unit = row["unit"].Trim(),
					OldValueKnown = parseBool(row["old_value_known"]),
					CurrentAppScope = parseBool(row["current_app_scope"]),
					ReviewPriority = row["review_priority"].Trim(),
					AppAction = row["app_action"].Trim(),
					SourceUrl = row["source_url"].Trim(),
					Notes = row["notes"].Trim()
				});
			}

			return changes;
		}

		private static StatScope parseScope(string value)
		{
			switch ((value ?? "").Trim().ToLowerInvariant())
			{
				case "initial": return StatScope.INITIAL;
				case "base": return StatScope.BASE;
				case "max_level_base": return StatScope.MAX_LEVEL_BASE;
				case "awakened": return StatScope.AWAKENED;
				case "awakened_base": return StatScope.AWAKENED_BASE;
				case "awakened_level_40": return StatScope.AWAKENED_LEVEL_40;
				case "awakening_bonus": return StatScope.AWAKENING_BONUS;
				case "unknown": return StatScope.UNKNOWN;
				default: throw new FormatException("Unknown stat scope: " + value);
			}
		}

		private static DateTime parseDate(string value)
		{
			DateTime result;

			if (!DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
			{
				throw new FormatException("Invalid date: " + value);
			}

			return result;
		}

		private static double parseDouble(string value)
		{
			double result;

			if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
			{
				throw new FormatException("Invalid number: " + value);
			}

			return result;
		}

		private static double? parseNullableDouble(string value)
		{
			return string.IsNullOrWhiteSpace(value) ? (double?)null : parseDouble(value);
		}

		private static bool parseBool(string value)
		{
			bool result;

			if (!bool.TryParse(value, out result))
			{
				throw new FormatException("Invalid boolean: " + value);
			}

			return result;
		}

		private static string require(string value, string columnName)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				throw new FormatException(columnName + " is empty.");
			}

			return value.Trim();
		}
	}
}
