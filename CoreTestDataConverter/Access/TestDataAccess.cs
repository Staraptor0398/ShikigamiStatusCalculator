using CoreTestDataConverter.Access.Format;
using CoreTestDataConverter.Model.MitamaCalculator;
using CoreTestDataConverter.Model.StatusCalculator;
using SaveData.Model.Development;

namespace CoreTestDataConverter.Access
{
	public static class TestDataAccess
	{
		public static CalculationTestSource LoadCalculationTestSource(string filePath)
		{
			return JsonDataAccess.LoadCalculationTestSource(filePath);
		}

		public static void SaveMitamaCalculatorTestData(string filePath, MitamaCalculatorTestData testData)
		{
			JsonDataAccess.Save(filePath, testData);
		}

		public static void SaveStatusCalculatorTestData(string filePath, StatusCalculatorTestData testData)
		{
			JsonDataAccess.Save(filePath, testData);
		}
	}
}
