using SaveData.Model;
using SaveData.Model.Development;

namespace Gui.Converter
{
	public static class StatusConverter
	{
		public static StatusSaveData ToSaveData(StatusDto dto)
		{
			if (dto == null)
			{
				return null;
			}

			return new StatusSaveData
			{
				Attack = dto.Attack,
				HP = dto.HP,
				Defense = dto.Defense,
				Speed = dto.Speed,
				CritRate = dto.CritRate,
				CritDamage = dto.CritDamage,
				EffectHit = dto.EffectHit,
				EffectResist = dto.EffectResist
			};
		}

		public static StatusDto ToDto(StatusSaveData saveData)
		{
			if (saveData == null)
			{
				return null;
			}

			return new StatusDto
			{
				Attack = saveData.Attack,
				HP = saveData.HP,
				Defense = saveData.Defense,
				Speed = saveData.Speed,
				CritRate = saveData.CritRate,
				CritDamage = saveData.CritDamage,
				EffectHit = saveData.EffectHit,
				EffectResist = saveData.EffectResist
			};
		}

		public static FullStatusSaveData ToFullSaveData(StatusDto dto)
		{
			return new FullStatusSaveData
			{
				Attack = dto.Attack,
				HP = dto.HP,
				Defense = dto.Defense,
				Speed = dto.Speed,
				CritRate = dto.CritRate,
				CritDamage = dto.CritDamage,
				EffectHit = dto.EffectHit,
				EffectResist = dto.EffectResist,
				AdditionalAttackRate = dto.AdditionalAttackRate,
				AdditionalHpRate = dto.AdditionalHpRate,
				AdditionalDefenseRate = dto.AdditionalDefenseRate
			};
		}
	}
}
