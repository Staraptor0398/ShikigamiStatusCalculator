using ShikigamiDataAuditor.Model;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ShikigamiDataAuditor.Access
{
	public class ShikigamiDataCsvReader
	{
		private static readonly IReadOnlyList<string> HEADERS = new[]
		{
			"レア度", "式神名", "攻撃力", "HP", "防御力", "素早さ", "会心率", "会心DMG", "効果命中", "効果抵抗"
		};

		public IReadOnlyList<ShikigamiStatus> Read(string filePath)
		{
			CsvDocument document = CsvDocument.Load(filePath, HEADERS);

			List<ShikigamiStatus> statuses = new List<ShikigamiStatus>();

			foreach (IReadOnlyDictionary<string, string> row in document.Rows)
			{
				Dictionary<StatType, double> values = new Dictionary<StatType, double>
				{
					{ StatType.ATTACK, parseDouble(row["攻撃力"], "攻撃力") },
					{ StatType.HP, parseDouble(row["HP"], "HP") },
					{ StatType.DEFENSE, parseDouble(row["防御力"], "防御力") },
					{ StatType.SPEED, parseDouble(row["素早さ"], "素早さ") },
					{ StatType.CRITICAL_RATE, parseDouble(row["会心率"], "会心率") },
					{ StatType.CRITICAL_DAMAGE, parseDouble(row["会心DMG"], "会心DMG") },
					{ StatType.EFFECT_HIT, parseDouble(row["効果命中"], "効果命中") },
					{ StatType.EFFECT_RESIST, parseDouble(row["効果抵抗"], "効果抵抗") }
				};

				statuses.Add(new ShikigamiStatus
				{
					Rarity = require(row["レア度"], "レア度"),
					Name = require(row["式神名"], "式神名"),
					StatScope = StatScope.AWAKENED_LEVEL_40,
					Values = values
				});
			}

			return statuses;
		}

		private static double parseDouble(string value, string columnName)
		{
			double result;

			if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result) || double.IsNaN(result) || double.IsInfinity(result))
			{
				throw new FormatException($"{columnName} is not a valid number: {value}");
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
