using CoreTestDataConverter.Model;
using SaveData.Model;

namespace CoreTestDataConverter.Converter
{
	public static class StatValueConverter
	{
		public static StatValueTestData ToTestData(StatValueSaveData saveData)
		{
			if (saveData == null)
			{
				return null;
			}

			return new StatValueTestData
			{
				Type = StatTypeConverter.ToStatType(saveData.Type),
				Value = saveData.Value
			};
		}
	}
}
