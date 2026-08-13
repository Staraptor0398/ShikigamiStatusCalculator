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
				case DisplayText.ADDITIONAL_ATTACK_RATE:
				case DisplayText.ADDITIONAL_HP_RATE:
				case DisplayText.CRITICAL_RATE:
				case DisplayText.EFFECT_HIT:
				case DisplayText.EFFECT_RESIST:
					setEffectValue = 15.0;
					break;
				case DisplayText.CRITICAL_DAMAGE:
					setEffectValue = 20.0;
					break;
				case DisplayText.ADDITIONAL_DEFENSE_RATE:
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
				case DisplayText.ADDITIONAL_ATTACK_RATE:
				case DisplayText.ADDITIONAL_HP_RATE:
				case DisplayText.CRITICAL_RATE:
				case DisplayText.EFFECT_HIT:
				case DisplayText.EFFECT_RESIST:
					uniqueEffectValue = 8.0;
					break;
				case DisplayText.ADDITIONAL_DEFENSE_RATE:
					uniqueEffectValue = 16.0;
					break;
				default:
					break;
			}

			return uniqueEffectValue;
		}
	}
}
