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
		public void Validate_CheckCleared()
		{
			var validator = new ScenarioValidator();
			var steps = new List<ScenarioStep>
			{
				new ScenarioStep(2, ScenarioCommandType.CHECK_CLEARED, Array.Empty<string>(), "CHECK CLEARED")
			};
			var scenario = new Scenario("Test.scenario", 1, 3, steps);

			validator.Validate(scenario);
		}

		[TestMethod]
		public void Validate_CheckClearedWithArgument()
		{
			var validator = new ScenarioValidator();
			var steps = new List<ScenarioStep>
			{
				new ScenarioStep(2, ScenarioCommandType.CHECK_CLEARED, new[] { "HOGE" }, "CHECK CLEARED HOGE")
			};
			var scenario = new Scenario("Test.scenario", 1, 3, steps);

			try
			{
				validator.Validate(scenario);
				Assert.Fail("ScenarioValidationException was not thrown.");
			}
			catch (ScenarioValidationException ex)
			{
				Assert.AreEqual("Too many arguments at line 2: CHECK CLEARED HOGE", ex.Message);
			}
		}

		[TestMethod]
		public void Validate_SaveMitama()
		{
			var validator = new ScenarioValidator();
			var steps = new List<ScenarioStep>
			{
				new ScenarioStep(2, ScenarioCommandType.SAVE_MITAMA, Array.Empty<string>(), "SAVE MITAMA")
			};
			var scenario = new Scenario("Test.scenario", 1, 3, steps);

			validator.Validate(scenario);
		}

		[TestMethod]
		public void Validate_SaveMitamaWithArgument()
		{
			var validator = new ScenarioValidator();
			var steps = new List<ScenarioStep>
			{
				new ScenarioStep(2, ScenarioCommandType.SAVE_MITAMA, new[] { "HOGE" }, "SAVE MITAMA HOGE")
			};
			var scenario = new Scenario("Test.scenario", 1, 3, steps);

			try
			{
				validator.Validate(scenario);
				Assert.Fail("ScenarioValidationException was not thrown.");
			}
			catch (ScenarioValidationException ex)
			{
				Assert.AreEqual("Too many arguments at line 2: SAVE MITAMA HOGE", ex.Message);
			}
		}

		[TestMethod]
		public void Validate_SaveBuild()
		{
			var validator = new ScenarioValidator();
			var steps = new List<ScenarioStep>
			{
				new ScenarioStep(2, ScenarioCommandType.SAVE_BUILD, Array.Empty<string>(), "SAVE BUILD")
			};
			var scenario = new Scenario("Test.scenario", 1, 3, steps);

			validator.Validate(scenario);
		}

		[TestMethod]
		public void Validate_SaveSnapshotBase()
		{
			var validator = new ScenarioValidator();
			var steps = new List<ScenarioStep>
			{
				new ScenarioStep(2, ScenarioCommandType.SAVE_SNAPSHOT, new[] { "BASE" }, "SAVE SNAPSHOT BASE")
			};
			var scenario = new Scenario("Test.scenario", 1, 3, steps);

			validator.Validate(scenario);
		}

		[TestMethod]
		public void Validate_SaveSnapshotTarget()
		{
			var validator = new ScenarioValidator();
			var steps = new List<ScenarioStep>
			{
				new ScenarioStep(2, ScenarioCommandType.SAVE_SNAPSHOT, new[] { "TARGET" }, "SAVE SNAPSHOT TARGET")
			};
			var scenario = new Scenario("Test.scenario", 1, 3, steps);

			validator.Validate(scenario);
		}

		[TestMethod]
		public void Validate_SaveSnapshotWithoutTarget()
		{
			var validator = new ScenarioValidator();
			var steps = new List<ScenarioStep>
			{
				new ScenarioStep(2, ScenarioCommandType.SAVE_SNAPSHOT, Array.Empty<string>(), "SAVE SNAPSHOT")
			};
			var scenario = new Scenario("Test.scenario", 1, 3, steps);

			try
			{
				validator.Validate(scenario);
				Assert.Fail("ScenarioValidationException was not thrown.");
			}
			catch (ScenarioValidationException ex)
			{
				Assert.AreEqual("Argument is missing at line 2: SAVE SNAPSHOT", ex.Message);
			}
		}

		[TestMethod]
		public void Validate_SaveSnapshotUnknownTarget()
		{
			var validator = new ScenarioValidator();
			var steps = new List<ScenarioStep>
			{
				new ScenarioStep(2, ScenarioCommandType.SAVE_SNAPSHOT, new[] { "HOGE" }, "SAVE SNAPSHOT HOGE")
			};
			var scenario = new Scenario("Test.scenario", 1, 3, steps);

			try
			{
				validator.Validate(scenario);
				Assert.Fail("ScenarioValidationException was not thrown.");
			}
			catch (ScenarioValidationException ex)
			{
				Assert.AreEqual(
					"Unknown SAVE SNAPSHOT target at line 2: SAVE SNAPSHOT HOGE",
					ex.Message);
			}
		}

		[TestMethod]
		public void Validate_SaveSnapshotWithTooManyArguments()
		{
			var validator = new ScenarioValidator();
			var steps = new List<ScenarioStep>
			{
				new ScenarioStep(2, ScenarioCommandType.SAVE_SNAPSHOT, new[] { "BASE", "HOGE" }, "SAVE SNAPSHOT BASE HOGE")
			};
			var scenario = new Scenario("Test.scenario", 1, 3, steps);

			try
			{
				validator.Validate(scenario);
				Assert.Fail("ScenarioValidationException was not thrown.");
			}
			catch (ScenarioValidationException ex)
			{
				Assert.AreEqual("Too many arguments at line 2: SAVE SNAPSHOT BASE HOGE", ex.Message);
			}
		}

		[TestMethod]
		public void Validate_LoadSavedMitama()
		{
			var validator = new ScenarioValidator();
			var steps = new List<ScenarioStep>
			{
				new ScenarioStep(2, ScenarioCommandType.LOAD_SAVED_MITAMA, Array.Empty<string>(), "LOAD SAVED MITAMA")
			};
			var scenario = new Scenario("Test.scenario", 1, 3, steps);

			validator.Validate(scenario);
		}

		[TestMethod]
		public void Validate_LoadSavedBuild()
		{
			var validator = new ScenarioValidator();
			var steps = new List<ScenarioStep>
			{
				new ScenarioStep(2, ScenarioCommandType.LOAD_SAVED_BUILD, Array.Empty<string>(), "LOAD SAVED BUILD")
			};
			var scenario = new Scenario("Test.scenario", 1, 3, steps);

			validator.Validate(scenario);
		}

		[TestMethod]
		public void Validate_CompareSavedSnapshot()
		{
			var validator = new ScenarioValidator();
			var steps = new List<ScenarioStep>
			{
				new ScenarioStep(2, ScenarioCommandType.COMPARE_SAVED_SNAPSHOT, Array.Empty<string>(), "COMPARE SAVED SNAPSHOT")
			};
			var scenario = new Scenario("Test.scenario", 1, 3, steps);

			validator.Validate(scenario);
		}

		[TestMethod]
		public void Validate_CompareSavedSnapshotWithArgument()
		{
			var validator = new ScenarioValidator();
			var steps = new List<ScenarioStep>
			{
				new ScenarioStep(2, ScenarioCommandType.COMPARE_SAVED_SNAPSHOT, new[] { "HOGE" }, "COMPARE SAVED SNAPSHOT HOGE")
			};
			var scenario = new Scenario("Test.scenario", 1, 3, steps);

			try
			{
				validator.Validate(scenario);
				Assert.Fail("ScenarioValidationException was not thrown.");
			}
			catch (ScenarioValidationException ex)
			{
				Assert.AreEqual("Too many arguments at line 2: COMPARE SAVED SNAPSHOT HOGE", ex.Message);
			}
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

		[TestMethod]
		public void Validate_ClearShikigami()
		{
			var validator = new ScenarioValidator();
			var steps = new List<ScenarioStep>
			{
				new ScenarioStep(2, ScenarioCommandType.CLEAR_SHIKIGAMI, Array.Empty<string>(), "CLEAR SHIKIGAMI")
			};
			var scenario = new Scenario("Test.scenario", 1, 3, steps);

			validator.Validate(scenario);
		}

		[TestMethod]
		public void Validate_ClearShikigamiWithArgument()
		{
			var validator = new ScenarioValidator();
			var steps = new List<ScenarioStep>
			{
				new ScenarioStep(2, ScenarioCommandType.CLEAR_SHIKIGAMI, new[] { "HOGE" }, "CLEAR SHIKIGAMI HOGE")
			};
			var scenario = new Scenario("Test.scenario", 1, 3, steps);

			try
			{
				validator.Validate(scenario);
				Assert.Fail("ScenarioValidationException was not thrown.");
			}
			catch (ScenarioValidationException ex)
			{
				Assert.AreEqual("Too many arguments at line 2: CLEAR SHIKIGAMI HOGE", ex.Message);
			}
		}

		[TestMethod]
		public void Validate_OpenCalcDetail()
		{
			var validator = new ScenarioValidator();
			var steps = new List<ScenarioStep>
			{
				new ScenarioStep(2, ScenarioCommandType.OPEN_CALC_DETAIL, Array.Empty<string>(), "OPEN CALC DETAIL")
			};
			var scenario = new Scenario("Test.scenario", 1, 3, steps);

			validator.Validate(scenario);
		}

		[TestMethod]
		public void Validate_CheckCalcDetail()
		{
			var validator = new ScenarioValidator();
			var steps = new List<ScenarioStep>
			{
				new ScenarioStep(2, ScenarioCommandType.CHECK_CALC_DETAIL, Array.Empty<string>(), "CHECK CALC DETAIL")
			};
			var scenario = new Scenario("Test.scenario", 1, 3, steps);

			validator.Validate(scenario);
		}

		[TestMethod]
		public void Validate_CloseCalcDetail()
		{
			var validator = new ScenarioValidator();
			var steps = new List<ScenarioStep>
			{
				new ScenarioStep(2, ScenarioCommandType.CLOSE_CALC_DETAIL, Array.Empty<string>(), "CLOSE CALC DETAIL")
			};
			var scenario = new Scenario("Test.scenario", 1, 3, steps);

			validator.Validate(scenario);
		}

		[TestMethod]
		public void Validate_OpenCalcDetailWithArgument()
		{
			var validator = new ScenarioValidator();
			var steps = new List<ScenarioStep>
			{
				new ScenarioStep(2, ScenarioCommandType.OPEN_CALC_DETAIL, new[] { "HOGE" }, "OPEN CALC DETAIL HOGE")
			};
			var scenario = new Scenario("Test.scenario", 1, 3, steps);

			try
			{
				validator.Validate(scenario);
				Assert.Fail("ScenarioValidationException was not thrown.");
			}
			catch (ScenarioValidationException ex)
			{
				Assert.AreEqual("Too many arguments at line 2: OPEN CALC DETAIL HOGE", ex.Message);
			}
		}

		[TestMethod]
		public void Validate_CheckCalcDetailWithArgument()
		{
			var validator = new ScenarioValidator();
			var steps = new List<ScenarioStep>
			{
				new ScenarioStep(2, ScenarioCommandType.CHECK_CALC_DETAIL, new[] { "HOGE" }, "CHECK CALC DETAIL HOGE")
			};
			var scenario = new Scenario("Test.scenario", 1, 3, steps);

			try
			{
				validator.Validate(scenario);
				Assert.Fail("ScenarioValidationException was not thrown.");
			}
			catch (ScenarioValidationException ex)
			{
				Assert.AreEqual("Too many arguments at line 2: CHECK CALC DETAIL HOGE", ex.Message);
			}
		}

		[TestMethod]
		public void Validate_CloseCalcDetailWithArgument()
		{
			var validator = new ScenarioValidator();
			var steps = new List<ScenarioStep>
			{
				new ScenarioStep(2, ScenarioCommandType.CLOSE_CALC_DETAIL, new[] { "HOGE" }, "CLOSE CALC DETAIL HOGE")
			};
			var scenario = new Scenario("Test.scenario", 1, 3, steps);

			try
			{
				validator.Validate(scenario);
				Assert.Fail("ScenarioValidationException was not thrown.");
			}
			catch (ScenarioValidationException ex)
			{
				Assert.AreEqual("Too many arguments at line 2: CLOSE CALC DETAIL HOGE", ex.Message);
			}
		}
	}
}
