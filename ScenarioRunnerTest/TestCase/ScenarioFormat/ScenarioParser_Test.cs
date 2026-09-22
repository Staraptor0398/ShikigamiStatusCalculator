using ScenarioRunner.ScenarioFormat;

namespace ScenarioRunnerTest.TestCase.ScenarioFormat
{
	[TestClass]
	public class ScenarioParser_Test
	{
		[TestMethod]
		public void Parse_ValidScenario()
		{
			var parser = new ScenarioParser();
			string[] lines =
			{
				"START",
				"SEL SHIKIGAMI \"願紡縁結神\"",
				"END"
			};

			Scenario actual = parser.Parse("Test.scenario", lines);

			Assert.AreEqual(1, actual.StartLine);
			Assert.AreEqual(3, actual.EndLine);
			Assert.AreEqual(1, actual.Steps.Count);
			Assert.AreEqual(2, actual.Steps[0].LineNumber);
			Assert.AreEqual(ScenarioCommandType.SELECT_SHIKIGAMI, actual.Steps[0].CommandType);
			Assert.AreEqual(1, actual.Steps[0].Arguments.Count);
			Assert.AreEqual("願紡縁結神", actual.Steps[0].Arguments[0]);
		}

		[TestMethod]
		public void Parse_CheckCleared()
		{
			var parser = new ScenarioParser();
			string[] lines =
			{
				"START",
				"CHECK CLEARED",
				"END"
			};

			Scenario actual = parser.Parse("Test.scenario", lines);
			ScenarioStep step = actual.Steps[0];

			Assert.AreEqual(ScenarioCommandType.CHECK_CLEARED, step.CommandType);
			Assert.AreEqual(0, step.Arguments.Count);
		}

		[TestMethod]
		public void Parse_SaveCommands()
		{
			var parser = new ScenarioParser();
			string[] lines =
			{
				"START",
				"SAVE MITAMA",
				"SAVE BUILD",
				"SAVE SNAPSHOT BASE",
				"SAVE SNAPSHOT TARGET",
				"END"
			};

			Scenario actual = parser.Parse("Test.scenario", lines);

			Assert.AreEqual(4, actual.Steps.Count);

			Assert.AreEqual(ScenarioCommandType.SAVE_MITAMA, actual.Steps[0].CommandType);
			Assert.AreEqual(0, actual.Steps[0].Arguments.Count);

			Assert.AreEqual(ScenarioCommandType.SAVE_BUILD, actual.Steps[1].CommandType);
			Assert.AreEqual(0, actual.Steps[1].Arguments.Count);

			Assert.AreEqual(ScenarioCommandType.SAVE_SNAPSHOT, actual.Steps[2].CommandType);
			Assert.AreEqual(1, actual.Steps[2].Arguments.Count);
			Assert.AreEqual("BASE", actual.Steps[2].Arguments[0]);

			Assert.AreEqual(ScenarioCommandType.SAVE_SNAPSHOT, actual.Steps[3].CommandType);
			Assert.AreEqual(1, actual.Steps[3].Arguments.Count);
			Assert.AreEqual("TARGET", actual.Steps[3].Arguments[0]);
		}

		[TestMethod]
		public void Parse_LoadSavedCommands()
		{
			var parser = new ScenarioParser();
			string[] lines =
			{
				"START",
				"LOAD SAVED MITAMA",
				"LOAD SAVED BUILD",
				"END"
			};

			Scenario actual = parser.Parse("Test.scenario", lines);

			Assert.AreEqual(2, actual.Steps.Count);

			Assert.AreEqual(ScenarioCommandType.LOAD_SAVED_MITAMA, actual.Steps[0].CommandType);
			Assert.AreEqual(0, actual.Steps[0].Arguments.Count);

			Assert.AreEqual(ScenarioCommandType.LOAD_SAVED_BUILD, actual.Steps[1].CommandType);
			Assert.AreEqual(0, actual.Steps[1].Arguments.Count);
		}

		[TestMethod]
		public void Parse_CompareSavedSnapshot()
		{
			var parser = new ScenarioParser();
			string[] lines =
			{
				"START",
				"COMPARE SAVED SNAPSHOT",
				"END"
			};

			Scenario actual = parser.Parse("Test.scenario", lines);
			ScenarioStep step = actual.Steps[0];

			Assert.AreEqual(ScenarioCommandType.COMPARE_SAVED_SNAPSHOT, step.CommandType);
			Assert.AreEqual(0, step.Arguments.Count);
		}

		[TestMethod]
		public void Parse_UnquotedString()
		{
			var parser = new ScenarioParser();
			string[] lines =
			{
				"START",
				"SEL SHIKIGAMI 願紡縁結神",
				"END"
			};

			try
			{
				parser.Parse("Test.scenario", lines);
				Assert.Fail("FormatException was not thrown.");
			}
			catch (FormatException ex)
			{
				Assert.AreEqual("String argument must be enclosed in double quotes at line 2: SEL SHIKIGAMI 願紡縁結神", ex.Message);
			}
		}

		[TestMethod]
		public void Parse_UnclosedQuotedString()
		{
			var parser = new ScenarioParser();
			string[] lines =
			{
				"START",
				"SEL SHIKIGAMI \"願紡縁結神",
				"END"
			};

			try
			{
				parser.Parse("Test.scenario", lines);
				Assert.Fail("FormatException was not thrown.");
			}
			catch (FormatException ex)
			{
				Assert.AreEqual("Quoted string is not closed at line 2.", ex.Message);
			}
		}

		[TestMethod]
		public void Parse_EquipMitamaSub_EmptyQuotedStat()
		{
			var parser = new ScenarioParser();
			string[] lines =
			{
				"START",
				"EQUIP MITAMA SUB 2 1 \"\" \"5\"",
				"END"
			};

			Scenario actual = parser.Parse("Test.scenario", lines);
			ScenarioStep step = actual.Steps[0];

			Assert.AreEqual(ScenarioCommandType.EQUIP_MITAMA, step.CommandType);
			Assert.AreEqual(5, step.Arguments.Count);
			Assert.AreEqual("SUB", step.Arguments[0]);
			Assert.AreEqual("2", step.Arguments[1]);
			Assert.AreEqual("1", step.Arguments[2]);
			Assert.AreEqual(string.Empty, step.Arguments[3]);
			Assert.AreEqual("5", step.Arguments[4]);
		}

		[TestMethod]
		public void Parse_ClearShikigami()
		{
			var parser = new ScenarioParser();
			string[] lines =
			{
				"START",
				"CLEAR SHIKIGAMI",
				"END"
			};

			Scenario actual = parser.Parse("Test.scenario", lines);
			ScenarioStep step = actual.Steps[0];

			Assert.AreEqual(ScenarioCommandType.CLEAR_SHIKIGAMI, step.CommandType);
			Assert.AreEqual(0, step.Arguments.Count);
		}
	}
}
