using ShikigamiDataAuditor.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ShikigamiDataAuditor.External
{
	internal static class StatusTextParser
	{
		private static readonly IReadOnlyDictionary<StatType, string[]> ALIASES =
			new Dictionary<StatType, string[]>
			{
				{ StatType.ATTACK, new[] { "攻击", "攻擊" } },
				{ StatType.HP, new[] { "生命" } },
				{ StatType.DEFENSE, new[] { "防御", "防禦" } },
				{ StatType.SPEED, new[] { "速度" } },
				{ StatType.CRITICAL_RATE, new[] { "暴击", "暴擊" } },
				{ StatType.CRITICAL_DAMAGE, new[] { "暴击伤害", "暴擊傷害", "暴伤", "暴傷" } },
				{ StatType.EFFECT_HIT, new[] { "效果命中" } },
				{ StatType.EFFECT_RESIST, new[] { "效果抵抗" } }
			};

		public static IReadOnlyList<ShikigamiStatus> Parse(string text, string rarity, string shikigamiName)
		{
			Dictionary<StatScope, Dictionary<StatType, double>> valuesByScope = new Dictionary<StatScope, Dictionary<StatType, double>>();

			foreach (string originalLine in (text ?? "").Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
			{
				string line = normalize(originalLine);
				StatScope scope = resolveScope(line);

				foreach (KeyValuePair<StatType, string[]> alias in ALIASES)
				{
					if (alias.Key == StatType.CRITICAL_RATE && containsAny(line, "暴击伤害", "暴擊傷害", "暴伤", "暴傷"))
					{
						continue;
					}

					double value;

					if (!tryReadValue(line, alias.Value, out value))
					{
						continue;
					}

					Dictionary<StatType, double> values;

					if (!valuesByScope.TryGetValue(scope, out values))
					{
						values = new Dictionary<StatType, double>();
						valuesByScope.Add(scope, values);
					}
					values[alias.Key] = value;
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

		private static bool tryReadValue(string line, IEnumerable<string> aliases, out double value)
		{
			value = 0;

			foreach (string alias in aliases.OrderByDescending(item => item.Length))
			{
				int index = line.IndexOf(alias, StringComparison.OrdinalIgnoreCase);

				if (index < 0)
				{
					continue;
				}

				string tail = line.Substring(index + alias.Length);
				MatchCollection matches = Regex.Matches(tail, @"[-+]?\d+(?:\.\d+)?\s*%?");

				if (matches.Count == 0)
				{
					continue;
				}

				string number = Regex.Match(matches[0].Value, @"[-+]?\d+(?:\.\d+)?").Value;

				if (double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
				{
					return true;
				}
			}
			return false;
		}

		private static StatScope resolveScope(string line)
		{
			bool awakened = containsAny(line, "觉醒", "覺醒");
			bool maxLevel = containsAny(line, "40级", "40級", "满级", "滿級", "最大等级", "最大等級");

			if (awakened && maxLevel)
			{
				return StatScope.AWAKENED_LEVEL_40;
			}
			if (maxLevel)
			{
				return StatScope.MAX_LEVEL_BASE;
			}
			if (awakened)
			{
				return StatScope.AWAKENED;
			}
			if (containsAny(line, "初始", "初期", "一级", "一級", "1级", "1級"))
			{
				return StatScope.INITIAL;
			}

			return StatScope.UNKNOWN;
		}

		private static bool containsAny(string value, params string[] candidates)
		{
			return candidates.Any(candidate => value.IndexOf(candidate, StringComparison.OrdinalIgnoreCase) >= 0);
		}

		private static string normalize(string value)
		{
			StringBuilder builder = new StringBuilder(value.Length);

			foreach (char character in value)
			{
				if (character >= '０' && character <= '９')
				{
					builder.Append((char)('0' + character - '０'));
				}
				else if (character == '．')
				{
					builder.Append('.');
				}
				else if (character == '％')
				{
					builder.Append('%');
				}
				else
				{
					builder.Append(character);
				}
			}

			return builder.ToString();
		}
	}
}
