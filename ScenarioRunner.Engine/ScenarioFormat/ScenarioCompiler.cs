namespace ScenarioRunner.ScenarioFormat
{

	public class ScenarioCompiler
	{
		private readonly ScenarioParser mParser;
		private readonly ScenarioValidator mValidator;

		public ScenarioCompiler()
		{
			mParser = new ScenarioParser();
			mValidator = new ScenarioValidator();
		}

		public Scenario Compile(string filePath, string[] lines)
		{
			Scenario scenario = mParser.Parse(filePath, lines);
			mValidator.Validate(scenario);

			return scenario;
		}
	}

}
