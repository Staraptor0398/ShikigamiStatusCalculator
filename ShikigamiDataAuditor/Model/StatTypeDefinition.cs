using System;
using System.Collections.Generic;

namespace ShikigamiDataAuditor.Model
{
	public static class StatTypeDefinition
	{
		private static readonly IReadOnlyDictionary<StatType, string> DISPLAY_NAMES = new Dictionary<StatType, string>
			{
				{ StatType.UNKNOWN, "不明" },
				{ StatType.ATTACK, "攻撃力" },
				{ StatType.HP, "HP" },
				{ StatType.DEFENSE, "防御力" },
				{ StatType.SPEED, "素早さ" },
				{ StatType.CRITICAL_RATE, "会心率" },
				{ StatType.CRITICAL_DAMAGE, "会心DMG" },
				{ StatType.EFFECT_HIT, "効果命中" },
				{ StatType.EFFECT_RESIST, "効果抵抗" }
			};

		public static string GetDisplayName(StatType statType)
		{
			return DISPLAY_NAMES[statType];
		}

		public static StatType Parse(string value)
		{
			string normalized = (value ?? "").Trim().ToLowerInvariant();

			switch (normalized)
			{
				case "attack":
				case "攻撃力":
					return StatType.ATTACK;
				case "hp":
					return StatType.HP;
				case "defense":
				case "防御力":
					return StatType.DEFENSE;
				case "speed":
				case "素早さ":
					return StatType.SPEED;
				case "critical_rate":
				case "会心率":
					return StatType.CRITICAL_RATE;
				case "critical_damage":
				case "会心dmg":
					return StatType.CRITICAL_DAMAGE;
				case "effect_hit":
				case "効果命中":
					return StatType.EFFECT_HIT;
				case "effect_resist":
				case "effect_resistance":
				case "効果抵抗":
					return StatType.EFFECT_RESIST;
				default:
					throw new FormatException("Unknown stat type: " + value);
			}
		}
	}
}
