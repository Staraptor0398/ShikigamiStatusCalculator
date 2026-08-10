using Gui.Common;

namespace Gui.Resolver
{
	public static class MitamaEffectValueResolver
	{
		public static double ResolveSetEffect(string text)
		{
			double setEffectValue = 0.0;

			switch (text)
			{
				case DisplayText.AdditionalAttackRate:
				case DisplayText.AdditionalHPRate:
				case DisplayText.CriticalRate:
				case DisplayText.EffectHit:
				case DisplayText.EffectResist:
					setEffectValue = 15.0;
					break;
				case DisplayText.CriticalDamage:
					setEffectValue = 20.0;
					break;
				case DisplayText.AdditionalDefenseRate:
					setEffectValue = 30.0;
					break;
				default:
					break;
			}

			return setEffectValue;
		}

		public static double ResolveUniqueEffect(string text)
		{
			double uniqueEffectValue = 0.0;

			switch (text)
			{
				case DisplayText.AdditionalAttackRate:
				case DisplayText.AdditionalHPRate:
				case DisplayText.CriticalRate:
				case DisplayText.EffectHit:
				case DisplayText.EffectResist:
					uniqueEffectValue = 8.0;
					break;
				case DisplayText.AdditionalDefenseRate:
					uniqueEffectValue = 16.0;
					break;
				default:
					break;
			}

			return uniqueEffectValue;
		}
	}
}
