using Gui.Common;

namespace Gui.Formatter
{
	public static class StatusFormatter
	{
		public static string FormatMitamaSummary(StatusDto status)
		{
			if (status == null)
			{
				return "";
			}

			return
					$"{DisplayText.ATTACK}: {status.Attack:F2} " +
					$"{DisplayText.HP}: {status.HP:F2} " +
					$"{DisplayText.DEFENCSE}: {status.Defense:F2} " +
					$"{DisplayText.SPEED}: {status.Speed:F2} " +

					$"{DisplayText.ADDITIONAL_ATTACK_RATE}: {status.AdditionalAttackRate:F2}% " +
					$"{DisplayText.ADDITIONAL_HP_RATE}: {status.AdditionalHpRate:F2}% " +
					$"{DisplayText.ADDITIONAL_DEFENSE_RATE}: {status.AdditionalDefenseRate:F2}% " +

					$"{DisplayText.CRITICAL_RATE}: {status.CritRate:F2}% " +
					$"{DisplayText.CRITICAL_DAMAGE}: {status.CritDamage:F2}% " +
					$"{DisplayText.EFFECT_HIT}: {status.EffectHit:F2}% " +
					$"{DisplayText.EFFECT_RESIST}: {status.EffectResist:F2}%";
		}

		public static string FormatFinalSummary(StatusDto status)
		{
			if (status == null)
			{
				return "";
			}

			return
					$"{DisplayText.ATTACK}: {status.Attack:F2} " +
					$"{DisplayText.HP}: {status.HP:F2} " +
					$"{DisplayText.DEFENCSE}: {status.Defense:F2} " +
					$"{DisplayText.SPEED}: {status.Speed:F2} " +
					$"{DisplayText.CRITICAL_RATE}: {status.CritRate:F2}% " +
					$"{DisplayText.CRITICAL_DAMAGE}: {status.CritDamage:F2}% " +
					$"{DisplayText.EFFECT_HIT}: {status.EffectHit:F2}% " +
					$"{DisplayText.EFFECT_RESIST}: {status.EffectResist:F2}%";
		}

		public static string FormatMitamaDetail(StatusDto status)
		{
			if (status == null)
			{
				return "";
			}

			return
					$"{DisplayText.ATTACK,-8}: {status.Attack:F2}\r\n" +
					$"{DisplayText.HP,-11}: {status.HP:F2}\r\n" +
					$"{DisplayText.DEFENCSE,-8}: {status.Defense:F2}\r\n" +
					$"{DisplayText.SPEED,-8}: {status.Speed:F2}\r\n" +

					$"{DisplayText.ADDITIONAL_ATTACK_RATE,-6}: {status.AdditionalAttackRate:F2}%\r\n" +
					$"{DisplayText.ADDITIONAL_HP_RATE,-9}: {status.AdditionalHpRate:F2}%\r\n" +
					$"{DisplayText.ADDITIONAL_DEFENSE_RATE,-6}: {status.AdditionalDefenseRate:F2}%\r\n" +

					$"{DisplayText.CRITICAL_RATE,-8}: {status.CritRate:F2}%\r\n" +
					$"{DisplayText.CRITICAL_DAMAGE,-9}: {status.CritDamage:F2}%\r\n" +
					$"{DisplayText.EFFECT_HIT,-7}: {status.EffectHit:F2}%\r\n" +
					$"{DisplayText.EFFECT_RESIST,-7}: {status.EffectResist:F2}%";
		}

		public static string FormatFinalDetail(StatusDto status)
		{
			if (status == null)
			{
				return "";
			}

			return
					$"{DisplayText.ATTACK,-8}: {status.Attack:F2}\r\n" +
					$"{DisplayText.HP,-11}: {status.HP:F2}\r\n" +
					$"{DisplayText.DEFENCSE,-8}: {status.Defense:F2}\r\n" +
					$"{DisplayText.SPEED,-8}: {status.Speed:F2}\r\n" +
					$"{DisplayText.CRITICAL_RATE,-8}: {status.CritRate:F2}%\r\n" +
					$"{DisplayText.CRITICAL_DAMAGE,-9}: {status.CritDamage:F2}%\r\n" +
					$"{DisplayText.EFFECT_HIT,-7}: {status.EffectHit:F2}%\r\n" +
					$"{DisplayText.EFFECT_RESIST,-7}: {status.EffectResist:F2}%";
		}

		public static string FormatBaseSummary(StatusDto status)
		{
			if (status == null)
			{
				return "";
			}

			return
					$"{DisplayText.ATTACK}: {status.Attack:F2} " +
					$"{DisplayText.HP}: {status.HP:F2} " +
					$"{DisplayText.DEFENCSE}: {status.Defense:F2} " +
					$"{DisplayText.SPEED}: {status.Speed:F2} " +
					$"{DisplayText.CRITICAL_RATE}: {status.CritRate:F2}% " +
					$"{DisplayText.CRITICAL_DAMAGE}: {status.CritDamage:F2}% " +
					$"{DisplayText.EFFECT_HIT}: {status.EffectHit:F2}% " +
					$"{DisplayText.EFFECT_RESIST}: {status.EffectResist:F2}%";
		}
	}
}
