using SaveData.Access.Development;
using SaveData.Model.Development;

namespace CoreTestDataConverter.Access.Format
{
	internal static class JsonDataAccess
	{
		public static CalculationTestSource LoadCalculationTestSource(string filePath)
		{
			return CalculationTestSourceAccess.Load(filePath);
		}

		public static void Save<T>(string filePath, T data)
		{
			SaveData.Access.JsonDataAccess.Save<T>(filePath, data);
		}
	}
}
