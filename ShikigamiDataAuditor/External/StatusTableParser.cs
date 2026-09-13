using ShikigamiDataAuditor.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace ShikigamiDataAuditor.External
{
	internal static class StatusTableParser
	{
		private const int MIN_USABLE_STAT_COUNT = 3;

		public static IReadOnlyList<ShikigamiStatus> Parse(IEnumerable<IReadOnlyList<string>> rows, string rarity, string shikigamiName)
		{
			List<IReadOnlyList<string>> rowList = rows.ToList();

			IReadOnlyList<ShikigamiStatus> verticalStatuses = parseVerticalTable(rowList, rarity, shikigamiName);

			if (hasUsableStatus(verticalStatuses))
			{
				return verticalStatuses;
			}

			return parseHorizontalTable(rowList, rarity, shikigamiName);
		}

		private static IReadOnlyList<ShikigamiStatus> parseVerticalTable(IReadOnlyList<IReadOnlyList<string>> rows, string rarity, string shikigamiName)
		{
			Dictionary<int, StatScope> scopeColumns = new Dictionary<int, StatScope>();

			foreach (IReadOnlyList<string> row in rows)
			{
				Dictionary<int, StatScope> resolvedScopeColumns = resolveScopeColumns(row);
				if (resolvedScopeColumns.Count == 0)
				{
					continue;
				}

				scopeColumns = resolvedScopeColumns;
				break;
			}

			if (scopeColumns.Count == 0)
			{
				return new List<ShikigamiStatus>();
			}

			Dictionary<StatScope, Dictionary<StatType, double>> valuesByScope = new Dictionary<StatScope, Dictionary<StatType, double>>();

			foreach (IReadOnlyList<string> row in rows)
			{
				if (row.Count == 0)
				{
					continue;
				}

				if (!tryResolveStatType(clean(row[0]), out StatType statType))
				{
					continue;
				}

				foreach (KeyValuePair<int, StatScope> scopeColumn in scopeColumns)
				{
					if (scopeColumn.Key >= row.Count)
					{
						continue;
					}

					if (!tryParseNumber(row[scopeColumn.Key], out double value))
					{
						continue;
					}

					if (!valuesByScope.TryGetValue(scopeColumn.Value, out Dictionary<StatType, double> values))
					{
						values = new Dictionary<StatType, double>();
						valuesByScope.Add(scopeColumn.Value, values);
					}

					values[statType] = value;
				}
			}

			return valuesByScope
				.Where(pair => pair.Value.Count >= MIN_USABLE_STAT_COUNT)
				.Select(pair => new ShikigamiStatus
				{
					Rarity = rarity,
					Name = shikigamiName,
					StatScope = pair.Key,
					Values = pair.Value
				})
				.ToList();
		}

		private static IReadOnlyList<ShikigamiStatus> parseHorizontalTable(IReadOnlyList<IReadOnlyList<string>> rows, string rarity, string shikigamiName)
		{
			Dictionary<int, StatType> headers = new Dictionary<int, StatType>();
			Dictionary<StatScope, Dictionary<StatType, double>> valuesByScope = new Dictionary<StatScope, Dictionary<StatType, double>>();

			foreach (IReadOnlyList<string> row in rows)
			{
				if (row.Count == 0)
				{
					continue;
				}

				if (isAwakeningBonusRow(row))
				{
					continue;
				}

				Dictionary<int, StatType> rowHeaders = resolveHeaders(row);
				if (rowHeaders.Count > 0)
				{
					headers = rowHeaders;
					continue;
				}

				StatScope scope = resolveHorizontalRowScope(row);
				if (scope == StatScope.UNKNOWN || headers.Count == 0)
				{
					continue;
				}

				if (!valuesByScope.TryGetValue(scope, out Dictionary<StatType, double> values))
				{
					values = new Dictionary<StatType, double>();
					valuesByScope.Add(scope, values);
				}

				foreach (KeyValuePair<int, StatType> header in headers)
				{
					if (header.Key >= row.Count)
					{
						continue;
					}

					if (!tryParseNumber(row[header.Key], out double value))
					{
						continue;
					}

					values[header.Value] = value;
				}
			}

			return valuesByScope
				.Where(pair => pair.Value.Count > 0)
				.Select(pair => new ShikigamiStatus
				{
					Rarity = rarity,
					Name = shikigamiName,
					StatScope = pair.Key,
					Values = pair.Value
				})
				.ToList();
		}

		private static Dictionary<int, StatScope> resolveScopeColumns(IReadOnlyList<string> row)
		{
			Dictionary<int, StatScope> scopeColumns = new Dictionary<int, StatScope>();

			for (int index = 0; index < row.Count; index++)
			{
				StatScope scope = resolveScope(row[index]);
				if (scope != StatScope.UNKNOWN)
				{
					scopeColumns[index] = scope;
				}
			}

			return scopeColumns;
		}

		private static Dictionary<int, StatType> resolveHeaders(IReadOnlyList<string> row)
		{
			Dictionary<int, StatType> headers = new Dictionary<int, StatType>();

			for (int index = 0; index < row.Count; index++)
			{
				if (tryResolveStatType(clean(row[index]), out StatType statType))
				{
					headers[index] = statType;
				}
			}

			return headers;
		}

		private static bool tryResolveStatType(string value, out StatType statType)
		{
			if (containsAny(value, "暴击伤害", "暴擊傷害", "暴伤", "暴傷"))
			{
				statType = StatType.CRITICAL_DAMAGE;
			}
			else if (containsAny(value, "效果命中"))
			{
				statType = StatType.EFFECT_HIT;
			}
			else if (containsAny(value, "效果抵抗"))
			{
				statType = StatType.EFFECT_RESIST;
			}
			else if (containsAny(value, "攻击", "攻擊"))
			{
				statType = StatType.ATTACK;
			}
			else if (containsAny(value, "生命"))
			{
				statType = StatType.HP;
			}
			else if (containsAny(value, "防御", "防禦"))
			{
				statType = StatType.DEFENSE;
			}
			else if (containsAny(value, "速度"))
			{
				statType = StatType.SPEED;
			}
			else if (containsAny(value, "暴击", "暴擊"))
			{
				statType = StatType.CRITICAL_RATE;
			}
			else
			{
				statType = StatType.UNKNOWN;
				return false;
			}

			return true;
		}

		private static StatScope resolveHorizontalRowScope(IReadOnlyList<string> row)
		{
			if (row.Count == 0)
			{
				return StatScope.UNKNOWN;
			}

			StatScope leadingScope = resolveScope(row[0]);

			if (leadingScope != StatScope.UNKNOWN)
			{
				return leadingScope;
			}

			return resolveScope(row);
		}

		private static StatScope resolveScope(IReadOnlyList<string> row)
		{
			return resolveScope(string.Join(" ", row.Select(clean)));
		}

		private static StatScope resolveScope(string value)
		{
			string normalized = clean(value);

			if (containsAny(normalized, "40级", "40級", "满级", "滿級", "最大等级", "最大等級", "满级数值", "滿級數值", "最大数值", "最大數值"))
			{
				return StatScope.AWAKENED_LEVEL_40;
			}

			if (containsAny(normalized, "未觉醒", "未覺醒"))
			{
				return StatScope.INITIAL;
			}

			if (containsAny(normalized, "初始", "初期", "1级", "1級"))
			{
				return StatScope.INITIAL;
			}

			if (containsAny(normalized, "觉醒", "覺醒"))
			{
				return StatScope.AWAKENED;
			}

			return StatScope.UNKNOWN;
		}

		private static bool isAwakeningBonusRow(IReadOnlyList<string> row)
		{
			if (row.Count == 0)
			{
				return false;
			}

			string leadingCell = clean(row[0]);
			return containsAny(leadingCell, "觉醒效果", "覺醒效果", "觉醒技能", "覺醒技能", "觉醒加成", "覺醒加成");
		}

		private static bool tryParseNumber(string value, out double result)
		{
			result = 0;

			string normalized = clean(value)
				.Replace("０", "0")
				.Replace("１", "1")
				.Replace("２", "2")
				.Replace("３", "3")
				.Replace("４", "4")
				.Replace("５", "5")
				.Replace("６", "6")
				.Replace("７", "7")
				.Replace("８", "8")
				.Replace("９", "9")
				.Replace("．", ".")
				.Replace(",", "");

			Match match = Regex.Match(normalized, @"[-+]?\d+(?:\.\d+)?");

			return match.Success && double.TryParse(match.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
		}

		private static bool hasUsableStatus(IReadOnlyList<ShikigamiStatus> statuses)
		{
			foreach (ShikigamiStatus status in statuses)
			{
				if (status.StatScope != StatScope.UNKNOWN && status.Values.Count >= MIN_USABLE_STAT_COUNT)
				{
					return true;
				}
			}

			return false;
		}

		private static string clean(string value)
		{
			return Regex.Replace(value ?? "", @"\s+", " ").Trim();
		}

		private static bool containsAny(string value, params string[] candidates)
		{
			return candidates.Any(candidate => value.IndexOf(candidate, StringComparison.OrdinalIgnoreCase) >= 0);
		}
	}
}
