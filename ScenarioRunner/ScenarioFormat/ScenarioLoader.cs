using System;
using System.IO;

namespace ScenarioRunner.ScenarioFormat
{
	public class ScenarioLoader
	{
		private readonly ScenarioParser mParser;
		private readonly ScenarioValidator mValidator;

		public ScenarioLoader()
		{
			mParser = new ScenarioParser();
			mValidator = new ScenarioValidator();
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
			Scenario scenario = mParser.Parse(filePath, lines);
			mValidator.Validate(scenario);
			return scenario;
		}
	}
}
