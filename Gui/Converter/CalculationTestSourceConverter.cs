using SaveData.Model.Development;

namespace Gui.Converter
{
	public static class CalculationTestSourceConverter
	{
		public static CalculationTestSource ToSaveData(StatusDto baseStatus, MitamaSetDto mitamaSet, CalculationResultDto result)
		{
			return new CalculationTestSource
			{
				BaseStatus = StatusConverter.ToSaveData(baseStatus),
				MitamaSet = MitamaSetConverter.ToSaveData(mitamaSet),
				MitamaStatus = StatusConverter.ToFullSaveData(result.MitamaOnlyStatus),
				FinalStatus = StatusConverter.ToSaveData(result.FinalStatus)
			};
		}
	}
}
