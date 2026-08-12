using CoreTestDataConverter.Model.StatusCalculator;
using SaveData.Model.Development;

namespace CoreTestDataConverter.Converter.StatusCalculator
{
	public static class StatusCalculatorTestDataConverter
	{
		public static StatusCalculatorTestData ToTestData(CalculationTestSource testSource)
		{
			if (testSource == null)
			{
				return null;
			}

			return new StatusCalculatorTestData
			{
				Input = new StatusCalculatorInputTestData
				{
					BaseStatus = StatusConverter.ToTestData(testSource.BaseStatus),
					MitamaStatus = StatusConverter.ToTestData(testSource.MitamaStatus)
				},
				Expected = StatusConverter.ToTestData(testSource.FinalStatus)
			};
		}
	}
}
