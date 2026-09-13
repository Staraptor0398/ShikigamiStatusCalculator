using ShikigamiDataAuditor.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace ShikigamiDataAuditor.Official
{
	public class OfficialStatChangeDiscovery
	{
		private static readonly Regex FROM_TO_PATTERN = new Regex(@"(?<old>\d+(?:\.\d+)?)\s*%?\s*(?:アップ)?\s*から\s*(?<new>\d+(?:\.\d+)?)\s*%?", RegexOptions.Compiled);
		private static readonly Regex NEW_VALUE_PATTERN = new Regex(@"(?:を|が)\s*(?<new>\d+(?:\.\d+)?)\s*%?\s*(?:に|まで)(?:調整|アップ|増加|変更)", RegexOptions.Compiled);
		private static readonly Regex BONUS_VALUE_PATTERN = new Regex(@"(?:を|が)\s*(?<new>\d+(?:\.\d+)?)\s*%\s*アップ(?:する|します|しました|される|されます)", RegexOptions.Compiled);
		private static readonly Regex INITIAL_VALUE_PATTERN = new Regex(@"初期[^、。]*?(?:を|が)\s*(?<new>\d+(?:\.\d+)?)\s*%?\s*(?:に|まで)(?:調整|アップ|増加|変更)", RegexOptions.Compiled);
		private static readonly Regex AWAKENED_VALUE_PATTERN = new Regex(@"覚醒後[^、。]*?(?:を|が)\s*(?<new>\d+(?:\.\d+)?)\s*%?\s*(?:に|まで)(?:調整|アップ|増加|変更)", RegexOptions.Compiled);
		private static readonly Regex HP_PATTERN = new Regex(@"(?:^|[：:\s])HP(?=[：:\sをが]|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		private const double VALUE_TOLERANCE = 0.000001;

		private readonly OfficialAdjustmentSectionParser mSectionParser;

		public OfficialStatChangeDiscovery(OfficialAdjustmentSectionParser sectionParser)
		{
			mSectionParser = sectionParser ?? throw new ArgumentNullException(nameof(sectionParser));
		}

		public IReadOnlyList<OfficialStatChange> Discover(OfficialAnnouncement announcement, IReadOnlyList<ShikigamiStatus> appStatuses)
		{
			if (announcement == null)
			{
				throw new ArgumentNullException(nameof(announcement));
			}

			if (appStatuses == null)
			{
				throw new ArgumentNullException(nameof(appStatuses));
			}

			IReadOnlyList<OfficialAdjustmentSection> sections = mSectionParser.Parse(announcement, appStatuses);
			List<OfficialStatChange> changes = new List<OfficialStatChange>();

			foreach (OfficialAdjustmentSection section in sections)
			{
				if (isNewlyIntroducedShikigami(announcement, section))
				{
					continue;
				}

				discoverSection(announcement, section, changes);
			}

			return removeDuplicates(changes);
		}

		private static bool isNewlyIntroducedShikigami(OfficialAnnouncement announcement, OfficialAdjustmentSection section)
		{
			foreach (string line in announcement.Lines)
			{
				if (!line.Contains("新登場"))
				{
					continue;
				}

				if (line.IndexOf(section.ShikigamiName, StringComparison.OrdinalIgnoreCase) >= 0)
				{
					return true;
				}
			}

			return false;
		}

		private static void discoverSection(OfficialAnnouncement announcement, OfficialAdjustmentSection section, ICollection<OfficialStatChange> changes)
		{
			for (int index = 0; index < section.Lines.Count; index++)
			{
				string line = section.Lines[index];

				StatType statType;

				if (!tryResolveStatType(line, out statType))
				{
					continue;
				}

				if (containsInitialAndAwakenedValues(line))
				{
					discoverInitialAndAwakenedValues(announcement, section, line, statType, changes);
					continue;
				}

				StatScope statScope = resolveStatScope(section.Lines, index, statType);

				if (statScope == StatScope.UNKNOWN)
				{
					continue;
				}

				double? oldValue;
				double newValue;

				if (!tryParseValue(line, statType, statScope, out oldValue, out newValue))
				{
					continue;
				}

				changes.Add(createChange(announcement, section, statScope, statType, oldValue, newValue));
			}
		}

		private static void discoverInitialAndAwakenedValues(OfficialAnnouncement announcement, OfficialAdjustmentSection section, string line, StatType statType, ICollection<OfficialStatChange> changes)
		{
			Match initialMatch = INITIAL_VALUE_PATTERN.Match(line);
			Match awakenedMatch = AWAKENED_VALUE_PATTERN.Match(line);

			if (initialMatch.Success)
			{
				double newValue = parseDouble(initialMatch.Groups["new"].Value);
				changes.Add(createChange(announcement, section, StatScope.INITIAL, statType, null, newValue));
			}

			if (awakenedMatch.Success)
			{
				double newValue = parseDouble(awakenedMatch.Groups["new"].Value);
				changes.Add(createChange(announcement, section, StatScope.AWAKENED_BASE, statType, null, newValue));
			}
		}

		private static bool containsInitialAndAwakenedValues(string line)
		{
			return line.Contains("初期") && line.Contains("覚醒後");
		}

		private static StatScope resolveStatScope(IReadOnlyList<string> lines, int lineIndex, StatType statType)
		{
			string line = lines[lineIndex];

			if (line.Contains("最大レベル") && isBaseStatLine(line, statType))
			{
				return StatScope.MAX_LEVEL_BASE;
			}

			if (line.Contains("初期"))
			{
				return StatScope.INITIAL;
			}

			if (line.Contains("覚醒効果") || line.Contains("覚醒状態"))
			{
				return StatScope.AWAKENING_BONUS;
			}

			if (line.Contains("覚醒後"))
			{
				if (line.Contains("削除") && (statType == StatType.EFFECT_HIT || statType == StatType.EFFECT_RESIST))
				{
					return StatScope.AWAKENING_BONUS;
				}

				return StatScope.AWAKENED_BASE;
			}

			if (isBaseStatLine(line, statType))
			{
				return StatScope.BASE;
			}

			return resolveInheritedStatScope(lines, lineIndex);
		}

		private static bool isBaseStatLine(string line, StatType statType)
		{
			switch (statType)
			{
				case StatType.ATTACK:
					return line.Contains("基礎攻撃力") || line.Contains("基礎攻撃");
				case StatType.HP:
					return line.Contains("基礎HP");
				case StatType.DEFENSE:
					return line.Contains("基礎防御力") || line.Contains("基礎防御");
				case StatType.SPEED:
					return line.Contains("基礎素早さ");
				case StatType.CRITICAL_RATE:
					return line.Contains("基礎会心率");
				case StatType.CRITICAL_DAMAGE:
					return line.Contains("基礎会心ダメージ") || line.Contains("基礎会心DMG");
				case StatType.EFFECT_HIT:
					return line.Contains("基礎効果命中");
				case StatType.EFFECT_RESIST:
					return line.Contains("基礎効果抵抗");
				default:
					return false;
			}
		}

		private static StatScope resolveInheritedStatScope(IReadOnlyList<string> lines, int lineIndex)
		{
			for (int index = lineIndex - 1; index >= 0; index--)
			{
				string line = lines[index];

				if (!line.StartsWith("※"))
				{
					continue;
				}

				if (line.Contains("最大レベル基礎ステータス"))
				{
					return StatScope.MAX_LEVEL_BASE;
				}

				if (line.Contains("覚醒効果"))
				{
					return StatScope.AWAKENING_BONUS;
				}

				if (line.Contains("基礎ステータス"))
				{
					return StatScope.BASE;
				}

				return StatScope.UNKNOWN;
			}

			return StatScope.UNKNOWN;
		}

		private static bool tryResolveStatType(string line, out StatType statType)
		{
			if (line.Contains("会心ダメージ") || line.Contains("会心DMG"))
			{
				statType = StatType.CRITICAL_DAMAGE;
				return true;
			}

			if (line.Contains("会心率"))
			{
				statType = StatType.CRITICAL_RATE;
				return true;
			}

			if (line.Contains("効果命中"))
			{
				statType = StatType.EFFECT_HIT;
				return true;
			}

			if (line.Contains("効果抵抗"))
			{
				statType = StatType.EFFECT_RESIST;
				return true;
			}

			if (line.Contains("素早さ"))
			{
				statType = StatType.SPEED;
				return true;
			}

			if (line.Contains("防御力"))
			{
				statType = StatType.DEFENSE;
				return true;
			}

			if (line.Contains("攻撃力"))
			{
				statType = StatType.ATTACK;
				return true;
			}

			if (HP_PATTERN.IsMatch(line))
			{
				statType = StatType.HP;
				return true;
			}

			statType = StatType.UNKNOWN;
			return false;
		}

		private static bool tryParseValue(string line, StatType statType, StatScope statScope, out double? oldValue, out double newValue)
		{
			Match fromToMatch = FROM_TO_PATTERN.Match(line);

			if (fromToMatch.Success)
			{
				oldValue = parseDouble(fromToMatch.Groups["old"].Value);
				newValue = parseDouble(fromToMatch.Groups["new"].Value);
				return true;
			}

			if (statScope == StatScope.AWAKENING_BONUS)
			{
				Match bonusValueMatch = BONUS_VALUE_PATTERN.Match(line);

				if (bonusValueMatch.Success)
				{
					oldValue = null;
					newValue = parseDouble(bonusValueMatch.Groups["new"].Value);
					return true;
				}
			}

			Match newValueMatch = NEW_VALUE_PATTERN.Match(line);

			if (newValueMatch.Success)
			{
				oldValue = null;
				newValue = parseDouble(newValueMatch.Groups["new"].Value);
				return true;
			}

			if (line.Contains("削除") && statType == StatType.EFFECT_HIT)
			{
				oldValue = null;
				newValue = 0;
				return true;
			}

			oldValue = null;
			newValue = 0;
			return false;
		}

		private static OfficialStatChange createChange(OfficialAnnouncement announcement, OfficialAdjustmentSection section, StatScope statScope, StatType statType, double? oldValue, double newValue)
		{
			return new OfficialStatChange
			{
				EffectiveDate = announcement.EffectiveDate,
				AnnouncementDate = announcement.AnnouncementDate,
				Rarity = section.Rarity,
				ShikigamiName = section.ShikigamiName,
				StatScope = statScope,
				StatType = statType,
				OldValue = oldValue,
				NewValue = newValue,
				Unit = getUnit(statType),
				OldValueKnown = oldValue.HasValue,
				SourceUrl = announcement.SourceUrl
			};
		}

		private static IReadOnlyList<OfficialStatChange> removeDuplicates(IEnumerable<OfficialStatChange> changes)
		{
			List<OfficialStatChange> uniqueChanges = new List<OfficialStatChange>();

			foreach (OfficialStatChange change in changes)
			{
				if (uniqueChanges.Any(existing => isEquivalent(existing, change)))
				{
					continue;
				}

				uniqueChanges.Add(change);
			}

			return uniqueChanges;
		}

		private static bool isEquivalent(OfficialStatChange left, OfficialStatChange right)
		{
			if (!string.Equals(left.Rarity, right.Rarity, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			if (!string.Equals(left.ShikigamiName, right.ShikigamiName, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			if (left.StatScope != right.StatScope || left.StatType != right.StatType)
			{
				return false;
			}

			if (left.OldValueKnown != right.OldValueKnown)
			{
				return false;
			}

			if (left.OldValueKnown && !areEqual(left.OldValue.Value, right.OldValue.Value))
			{
				return false;
			}

			return areEqual(left.NewValue, right.NewValue);
		}

		private static bool areEqual(double left, double right)
		{
			return Math.Abs(left - right) <= VALUE_TOLERANCE;
		}

		private static string getUnit(StatType statType)
		{
			switch (statType)
			{
				case StatType.CRITICAL_RATE:
				case StatType.CRITICAL_DAMAGE:
				case StatType.EFFECT_HIT:
				case StatType.EFFECT_RESIST:
					return "percent";
				default:
					return "point";
			}
		}

		private static double parseDouble(string value)
		{
			return double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
		}
	}
}
