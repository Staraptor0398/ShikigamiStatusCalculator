using Gui.Common;

namespace Gui.Converter
{
	public static class StatTypeConverter
	{
		public static StatTypeDto ToDto(string text)
		{
			StatTypeDto dto = StatTypeDto.None;

			switch (text)
			{
				case DisplayText.ATTACK: dto = StatTypeDto.Attack; break;
				case DisplayText.HP: dto = StatTypeDto.Hp; break;
				case DisplayText.DEFENSE: dto = StatTypeDto.Defense; break;
				case DisplayText.SPEED: dto = StatTypeDto.Speed; break;
				case DisplayText.CRITICAL_RATE: dto = StatTypeDto.CriticalRate; break;
				case DisplayText.CRITICAL_DAMAGE: dto = StatTypeDto.CriticalDamage; break;
				case DisplayText.EFFECT_HIT: dto = StatTypeDto.EffectHit; break;
				case DisplayText.EFFECT_RESIST: dto = StatTypeDto.EffectResist; break;
				case DisplayText.ADDITIONAL_ATTACK_RATE: dto = StatTypeDto.AdditionalAttackRate; break;
				case DisplayText.ADDITIONAL_HP_RATE: dto = StatTypeDto.AdditionalHpRate; break;
				case DisplayText.ADDITIONAL_DEFENSE_RATE: dto = StatTypeDto.AdditionalDefenseRate; break;
				default: break;
			}

			return dto;
		}

		public static string ToText(StatTypeDto dto)
		{
			return dto.ToString();
		}
	}
}
