using CoreTestDataConverter.Model;
using System;

namespace CoreTestDataConverter.Converter
{
	public static class StatTypeConverter
	{
		public static StatType ToStatType(string value)
		{
			switch (value)
			{
				case "None":
					return StatType.None;
				case "Attack":
					return StatType.Attack;
				case "Hp":
					return StatType.Hp;
				case "Defense":
					return StatType.Defense;
				case "Speed":
					return StatType.Speed;
				case "CriticalRate":
					return StatType.CriticalRate;
				case "CriticalDamage":
					return StatType.CriticalDamage;
				case "EffectHit":
					return StatType.EffectHit;
				case "EffectResist":
					return StatType.EffectResist;
				case "AdditionalAttackRate":
					return StatType.AdditionalAttackRate;
				case "AdditionalHpRate":
					return StatType.AdditionalHpRate;
				case "AdditionalDefenseRate":
					return StatType.AdditionalDefenseRate;
				default:
					throw new ArgumentException($"Unknown stat type: {value}", nameof(value));
			}
		}
	}
}
