using CoreTestDataConverter.Model;
using SaveData.Model;
using SaveData.Model.Development;

namespace CoreTestDataConverter.Converter
{
	public static class StatusConverter
	{
		public static StatusTestData ToTestData(StatusSaveData saveData)
		{
			if (saveData == null)
			{
				return null;
			}

			return new StatusTestData
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

		public static FullStatusTestData ToTestData(FullStatusSaveData saveData)
		{
			if (saveData == null)
			{
				return null;
			}

			return new FullStatusTestData
			{
				Attack = saveData.Attack,
				HP = saveData.HP,
				Defense = saveData.Defense,
				Speed = saveData.Speed,
				CritRate = saveData.CritRate,
				CritDamage = saveData.CritDamage,
				EffectHit = saveData.EffectHit,
				EffectResist = saveData.EffectResist,
				AdditionalAttackRate = saveData.AdditionalAttackRate,
				AdditionalHpRate = saveData.AdditionalHpRate,
				AdditionalDefenseRate = saveData.AdditionalDefenseRate,
			};
		}
	}
}
