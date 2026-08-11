using SaveData.Definition.Development;
using SaveData.Model.Development;

using SaveData.Model.File.Development;
using System;

namespace SaveData.Access.Development
{
	public static class CalculationTestSourceAccess
	{
		public static void Save(string path, CalculationTestSource data)
		{
			TestSourceFile<CalculationTestSource> file = new TestSourceFile<CalculationTestSource>
			{
				Version = TestSourceVersionDefinition.Calculation.Version,
				Data = data
			};


			JsonDataAccess.Save(path, file);
		}

		public static CalculationTestSource Load(string path)
		{
			TestSourceFile<CalculationTestSource> file = JsonDataAccess.Load<TestSourceFile<CalculationTestSource>>(path);

			if (file == null)
			{
				return null;
			}

			if (file.Version > TestSourceVersionDefinition.Calculation.Version)
			{
				throw new NotSupportedException($"Unsupported calculation test source version: {file.Version}");
			}

			return file.Data;
		}
	}
}
