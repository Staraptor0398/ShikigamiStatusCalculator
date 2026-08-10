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
				case DisplayText.Attack: dto = StatTypeDto.Attack; break;
				case DisplayText.HP: dto = StatTypeDto.Hp; break;
				case DisplayText.Defense: dto = StatTypeDto.Defense; break;
				case DisplayText.Speed: dto = StatTypeDto.Speed; break;
				case DisplayText.CriticalRate: dto = StatTypeDto.CriticalRate; break;
				case DisplayText.CriticalDamage: dto = StatTypeDto.CriticalDamage; break;
				case DisplayText.EffectHit: dto = StatTypeDto.EffectHit; break;
				case DisplayText.EffectResist: dto = StatTypeDto.EffectResist; break;
				case DisplayText.AdditionalAttackRate: dto = StatTypeDto.AdditionalAttackRate; break;
				case DisplayText.AdditionalHPRate: dto = StatTypeDto.AdditionalHpRate; break;
				case DisplayText.AdditionalDefenseRate: dto = StatTypeDto.AdditionalDefenseRate; break;
				default: break;
			}

			return dto;
		}
	}
}
