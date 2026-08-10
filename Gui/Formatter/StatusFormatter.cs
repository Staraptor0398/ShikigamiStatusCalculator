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
					$"{DisplayText.Attack}: {status.Attack:F2} " +
					$"{DisplayText.HP}: {status.HP:F2} " +
					$"{DisplayText.Defense}: {status.Defense:F2} " +
					$"{DisplayText.Speed}: {status.Speed:F2} " +

					$"{DisplayText.AdditionalAttackRate}: {status.AdditionalAttackRate:F2}% " +
					$"{DisplayText.AdditionalHPRate}: {status.AdditionalHpRate:F2}% " +
					$"{DisplayText.AdditionalDefenseRate}: {status.AdditionalDefenseRate:F2}% " +

					$"{DisplayText.CriticalRate}: {status.CritRate:F2}% " +
					$"{DisplayText.CriticalDamage}: {status.CritDamage:F2}% " +
					$"{DisplayText.EffectHit}: {status.EffectHit:F2}% " +
					$"{DisplayText.EffectResist}: {status.EffectResist:F2}%";
		}

		public static string FormatFinalSummary(StatusDto status)
		{
			if (status == null)
			{
				return "";
			}

			return
					$"{DisplayText.Attack}: {status.Attack:F2} " +
					$"{DisplayText.HP}: {status.HP:F2} " +
					$"{DisplayText.Defense}: {status.Defense:F2} " +
					$"{DisplayText.Speed}: {status.Speed:F2} " +
					$"{DisplayText.CriticalRate}: {status.CritRate:F2}% " +
					$"{DisplayText.CriticalDamage}: {status.CritDamage:F2}% " +
					$"{DisplayText.EffectHit}: {status.EffectHit:F2}% " +
					$"{DisplayText.EffectResist}: {status.EffectResist:F2}%";
		}

		public static string FormatMitamaDetail(StatusDto status)
		{
			if (status == null)
			{
				return "";
			}

			return
					$"{DisplayText.Attack,-8}: {status.Attack:F2}\r\n" +
					$"{DisplayText.HP,-11}: {status.HP:F2}\r\n" +
					$"{DisplayText.Defense,-8}: {status.Defense:F2}\r\n" +
					$"{DisplayText.Speed,-8}: {status.Speed:F2}\r\n" +

					$"{DisplayText.AdditionalAttackRate,-6}: {status.AdditionalAttackRate:F2}%\r\n" +
					$"{DisplayText.AdditionalHPRate,-9}: {status.AdditionalHpRate:F2}%\r\n" +
					$"{DisplayText.AdditionalDefenseRate,-6}: {status.AdditionalDefenseRate:F2}%\r\n" +

					$"{DisplayText.CriticalRate,-8}: {status.CritRate:F2}%\r\n" +
					$"{DisplayText.CriticalDamage,-9}: {status.CritDamage:F2}%\r\n" +
					$"{DisplayText.EffectHit,-7}: {status.EffectHit:F2}%\r\n" +
					$"{DisplayText.EffectResist,-7}: {status.EffectResist:F2}%";
		}

		public static string FormatFinalDetail(StatusDto status)
		{
			if (status == null)
			{
				return "";
			}

			return
					$"{DisplayText.Attack,-8}: {status.Attack:F2}\r\n" +
					$"{DisplayText.HP,-11}: {status.HP:F2}\r\n" +
					$"{DisplayText.Defense,-8}: {status.Defense:F2}\r\n" +
					$"{DisplayText.Speed,-8}: {status.Speed:F2}\r\n" +
					$"{DisplayText.CriticalRate,-8}: {status.CritRate:F2}%\r\n" +
					$"{DisplayText.CriticalDamage,-9}: {status.CritDamage:F2}%\r\n" +
					$"{DisplayText.EffectHit,-7}: {status.EffectHit:F2}%\r\n" +
					$"{DisplayText.EffectResist,-7}: {status.EffectResist:F2}%";
		}
	}
}
