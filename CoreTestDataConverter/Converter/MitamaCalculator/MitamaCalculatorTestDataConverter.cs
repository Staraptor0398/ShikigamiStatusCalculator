using CoreTestDataConverter.Model.MitamaCalculator;
using SaveData.Model.Development;

namespace CoreTestDataConverter.Converter.MitamaCalculator
{
	public static class MitamaCalculatorTestDataConverter
	{
		public static MitamaCalculatorTestData ToTestData(CalculationTestSource testSource)
		{
			if (testSource == null)
			{
				return null;
			}

			return new MitamaCalculatorTestData
			{
				Input = MitamaSetConverter.ToTestData(testSource.MitamaSet),
				Expected = StatusConverter.ToTestData(testSource.MitamaStatus)
			};
		}
	}
}
