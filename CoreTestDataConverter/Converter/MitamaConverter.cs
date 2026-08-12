using CoreTestDataConverter.Model;
using SaveData.Model;
using System.Linq;

namespace CoreTestDataConverter.Converter
{
	public static class MitamaConverter
	{
		public static MitamaTestData ToTestData(MitamaSaveData saveData)
		{
			if (saveData == null)
			{
				return null;
			}

			return new MitamaTestData
			{
				MainStat = StatValueConverter.ToTestData(saveData.MainStat),
				SubStats = saveData.SubStats.Select(StatValueConverter.ToTestData).ToArray(),
			};
		}
	}
}
