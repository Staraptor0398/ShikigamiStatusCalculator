using Gui.SaveData;

namespace Gui.Converter
{
	public static class StatusSaveDataConverter
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
				Deffense = dto.Defense,
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
				Defense = saveData.Deffense,
				Speed = saveData.Speed,
				CritRate = saveData.CritRate,
				CritDamage = saveData.CritDamage,
				EffectHit = saveData.EffectHit,
				EffectResist = saveData.EffectResist
			};
		}
	}
}
