using System;
using System.IO;

namespace ScenarioRunner.ScenarioFormat
{
	public class ScenarioLoader
	{
		private readonly ScenarioCompiler mCompiler;

		public ScenarioLoader()
		{
			mCompiler = new ScenarioCompiler();
		}

		public Scenario Load(string filePath)
		{
			if (string.IsNullOrWhiteSpace(filePath))
			{
				throw new ArgumentException("Scenario file path is empty.", nameof(filePath));
			}
			if (!File.Exists(filePath))
			{
				throw new FileNotFoundException("Scenario file was not found.", filePath);
			}

			string[] lines = File.ReadAllLines(filePath);
			return mCompiler.Compile(filePath, lines);
		}
	}
}
