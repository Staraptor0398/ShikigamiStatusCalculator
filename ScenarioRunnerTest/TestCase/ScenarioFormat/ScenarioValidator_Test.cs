using ScenarioRunner.ScenarioFormat;

namespace ScenarioRunnerTest.TestCase.ScenarioFormat
{
	[TestClass]
	public class ScenarioValidator_Test
	{
		[TestMethod]
		public void Validate_ValidScenario()
		{
			var validator = new ScenarioValidator();
			var steps = new List<ScenarioStep>
			{
				new ScenarioStep(2, ScenarioCommandType.RECOVER_SHIKIGAMI, new[] { "BROKEN" }, "RECOVER SHIKIGAMI BROKEN")
			};
			var scenario = new Scenario("Test.scenario", 1, 3, steps);

			validator.Validate(scenario);
		}

		[TestMethod]
		public void Validate_RecoverShikigamiBackup()
		{
			var validator = new ScenarioValidator();
			var steps = new List<ScenarioStep>
			{
				new ScenarioStep(2, ScenarioCommandType.RECOVER_SHIKIGAMI, new[] { "BACKUP" }, "RECOVER SHIKIGAMI BACKUP")
			};
			var scenario = new Scenario("Test.scenario", 1, 3, steps);

			validator.Validate(scenario);
		}

		[TestMethod]
		public void Validate_RecoverShikigamiUnknownTarget()
		{
			var validator = new ScenarioValidator();
			var steps = new List<ScenarioStep>
			{
				new ScenarioStep(2, ScenarioCommandType.RECOVER_SHIKIGAMI, new[] { "HOGE" }, "RECOVER SHIKIGAMI HOGE")
			};
			var scenario = new Scenario("Test.scenario", 1, 3, steps);

			try
			{
				validator.Validate(scenario);
				Assert.Fail("ScenarioValidationException was not thrown.");
			}
			catch (ScenarioValidationException ex)
			{
				Assert.AreEqual("Unknown RECOVER SHIKIGAMI target at line 2: RECOVER SHIKIGAMI HOGE", ex.Message);
			}
		}

		[TestMethod]
		public void Validate_MissingStart()
		{
			var validator = new ScenarioValidator();
			var scenario = new Scenario("Test.scenario", -1, 2, new List<ScenarioStep>());

			try
			{
				validator.Validate(scenario);
				Assert.Fail("ScenarioValidationException was not thrown.");
			}
			catch (ScenarioValidationException ex)
			{
				Assert.AreEqual("START is not defined.", ex.Message);
			}
		}

		[TestMethod]
		public void Validate_MissingEnd()
		{
			var validator = new ScenarioValidator();
			var scenario = new Scenario("Test.scenario", 1, -1, new List<ScenarioStep>());

			try
			{
				validator.Validate(scenario);
				Assert.Fail("ScenarioValidationException was not thrown.");
			}
			catch (ScenarioValidationException ex)
			{
				Assert.AreEqual("END is not defined.", ex.Message);
			}
		}
	}
}
