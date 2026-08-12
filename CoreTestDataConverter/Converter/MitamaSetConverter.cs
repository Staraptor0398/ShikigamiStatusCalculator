using CoreTestDataConverter.Model;
using SaveData.Model;
using System.Linq;

namespace CoreTestDataConverter.Converter
{
	public static class MitamaSetConverter
	{
		public static MitamaSetTestData ToTestData(MitamaSetSaveData saveData)
		{
			if (saveData == null)
			{
				return null;
			}

			return new MitamaSetTestData
			{
				Mitamas = saveData.Mitamas.Select(MitamaConverter.ToTestData).ToArray(),
				SetEffects = saveData.SetEffects.Select(effect => StatValueConverter.ToTestData(effect.Stat)).ToArray(),
				UniqueEffects = saveData.UniqueEffects.Select(effect => StatValueConverter.ToTestData(effect.Stat)).ToArray()
			};
		}
	}
}
