
using ScenarioRunner.ScenarioFormat;

namespace ScenarioRunnerTest.TestCase.ScenarioFormat
{
	[TestClass]
	public class ScenarioValidator_Test
	{
		[TestMethod]
		[DataRow(ScenarioCommandType.OPEN_CLEAR_DIALOG, "OPEN CLEAR DIALOG")]
		[DataRow(ScenarioCommandType.CONFIRM_DIALOG, "CONFIRM DIALOG")]
		[DataRow(ScenarioCommandType.CANCEL_DIALOG, "CANCEL DIALOG")]
		public void Validate_DialogDecisionCommands(ScenarioCommandType type, string command)
		{
			var validator = new ScenarioValidator();
			validator.Validate(new Scenario("Test.scenario", 1, 3, new[]
			{
				new ScenarioStep(2, type, Array.Empty<string>(), command)
			}));

			try
			{
				validator.Validate(new Scenario("Test.scenario", 1, 3, new[]
				{
					new ScenarioStep(2, type, new[] { "EXTRA" }, command + " EXTRA")
				}));
				Assert.Fail("ScenarioValidationException was not thrown.");
			}
			catch (ScenarioValidationException ex)
			{
				Assert.AreEqual($"Too many arguments at line 2: {command} EXTRA", ex.Message);
			}
		}

		[TestMethod]
		[DataRow(ScenarioCommandType.CAPTURE_CALC_DETAIL, "CAPTURE CALC DETAIL")]
		[DataRow(ScenarioCommandType.CHECK_CALC_DETAIL_UNCHANGED, "CHECK CALC DETAIL UNCHANGED")]
		public void Validate_CalculationDetailCaptureCommands(ScenarioCommandType type, string command)
		{
			var validator = new ScenarioValidator();
			validator.Validate(new Scenario("Test.scenario", 1, 3, new[]
			{
				new ScenarioStep(2, type, Array.Empty<string>(), command)
			}));

			try
			{
				validator.Validate(new Scenario("Test.scenario", 1, 3, new[]
				{
					new ScenarioStep(2, type, new[] { "EXTRA" }, command + " EXTRA")
				}));
				Assert.Fail("ScenarioValidationException was not thrown.");
			}
			catch (ScenarioValidationException ex)
			{
				Assert.AreEqual($"Too many arguments at line 2: {command} EXTRA", ex.Message);
			}
		}

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

		[TestMethod]
		public void Validate_CheckSaveDataLevelNone()
		{
			var validator = new ScenarioValidator();
			var steps = new List<ScenarioStep>
			{
				new ScenarioStep(2, ScenarioCommandType.CHECK_SAVEDATA_LEVEL, new[] { "NONE" }, "CHECK SAVEDATA LEVEL NONE")
			};
			var scenario = new Scenario("Test.scenario", 1, 3, steps);

			validator.Validate(scenario);
		}

		[TestMethod]
		public void Validate_CheckSaveDataLevelMitama()
		{
			var validator = new ScenarioValidator();
			var steps = new List<ScenarioStep>
			{
				new ScenarioStep(2, ScenarioCommandType.CHECK_SAVEDATA_LEVEL, new[] { "MITAMA" }, "CHECK SAVEDATA LEVEL MITAMA")
			};
			var scenario = new Scenario("Test.scenario", 1, 3, steps);

			validator.Validate(scenario);
		}

		[TestMethod]
		public void Validate_CheckSaveDataLevelBuild()
		{
			var validator = new ScenarioValidator();
			var steps = new List<ScenarioStep>
			{
				new ScenarioStep(2, ScenarioCommandType.CHECK_SAVEDATA_LEVEL, new[] { "BUILD" }, "CHECK SAVEDATA LEVEL BUILD")
			};
			var scenario = new Scenario("Test.scenario", 1, 3, steps);

			validator.Validate(scenario);
		}

		[TestMethod]
		public void Validate_CheckSaveDataLevelSnapshot()
		{
			var validator = new ScenarioValidator();
			var steps = new List<ScenarioStep>
			{
				new ScenarioStep(2, ScenarioCommandType.CHECK_SAVEDATA_LEVEL, new[] { "SNAPSHOT" }, "CHECK SAVEDATA LEVEL SNAPSHOT")
			};
			var scenario = new Scenario("Test.scenario", 1, 3, steps);

			validator.Validate(scenario);
		}

		[TestMethod]
		public void Validate_CheckSaveDataLevelWithoutLevel()
		{
			var validator = new ScenarioValidator();
			var steps = new List<ScenarioStep>
			{
				new ScenarioStep(2, ScenarioCommandType.CHECK_SAVEDATA_LEVEL, Array.Empty<string>(), "CHECK SAVEDATA LEVEL")
			};
			var scenario = new Scenario("Test.scenario", 1, 3, steps);

			try
			{
				validator.Validate(scenario);
				Assert.Fail("ScenarioValidationException was not thrown.");
			}
			catch (ScenarioValidationException ex)
			{
				Assert.AreEqual("Argument is missing at line 2: CHECK SAVEDATA LEVEL", ex.Message);
			}
		}

		[TestMethod]
		public void Validate_CheckSaveDataLevelUnknownLevel()
		{
			var validator = new ScenarioValidator();
			var steps = new List<ScenarioStep>
			{
				new ScenarioStep(2, ScenarioCommandType.CHECK_SAVEDATA_LEVEL, new[] { "HOGE" }, "CHECK SAVEDATA LEVEL HOGE")
			};
			var scenario = new Scenario("Test.scenario", 1, 3, steps);

			try
			{
				validator.Validate(scenario);
				Assert.Fail("ScenarioValidationException was not thrown.");
			}
			catch (ScenarioValidationException ex)
			{
				Assert.AreEqual("Unknown CHECK SAVEDATA LEVEL target at line 2: CHECK SAVEDATA LEVEL HOGE", ex.Message);
			}
		}

		[TestMethod]
		public void Validate_CheckSaveDataLevelWithTooManyArguments()
		{
			var validator = new ScenarioValidator();
			var steps = new List<ScenarioStep>
			{
				new ScenarioStep(2, ScenarioCommandType.CHECK_SAVEDATA_LEVEL, new[] { "BUILD", "HOGE" }, "CHECK SAVEDATA LEVEL BUILD HOGE")
			};
			var scenario = new Scenario("Test.scenario", 1, 3, steps);

			try
			{
				validator.Validate(scenario);
				Assert.Fail("ScenarioValidationException was not thrown.");
			}
			catch (ScenarioValidationException ex)
			{
				Assert.AreEqual("Too many arguments at line 2: CHECK SAVEDATA LEVEL BUILD HOGE", ex.Message);
			}
		}

		[TestMethod]
		[DataRow("MAIN", "1", null, true)]
		[DataRow("MAIN", "6", null, true)]
		[DataRow("MAIN", "0", null, false)]
		[DataRow("MAIN", "7", null, false)]
		[DataRow("MAIN", "ABC", null, false)]
		[DataRow("MAIN", "2147483648", null, false)]
		[DataRow("SUB", "1", "1", true)]
		[DataRow("SUB", "6", "4", true)]
		[DataRow("SUB", "0", "1", false)]
		[DataRow("SUB", "7", "1", false)]
		[DataRow("SUB", "1", "0", false)]
		[DataRow("SUB", "1", "5", false)]
		[DataRow("SUB", "1", "ABC", false)]
		[DataRow("SET", "1", null, true)]
		[DataRow("SET", "3", null, true)]
		[DataRow("SET", "0", null, false)]
		[DataRow("SET", "4", null, false)]
		[DataRow("SET", "ABC", null, false)]
		[DataRow("UNIQUE", "1", null, true)]
		[DataRow("UNIQUE", "6", null, true)]
		[DataRow("UNIQUE", "0", null, false)]
		[DataRow("UNIQUE", "7", null, false)]
		[DataRow("UNIQUE", "ABC", null, false)]
		public void Validate_EquipMitamaSlotRange(string target, string slot, string subSlot, bool expectedValid)
		{
			string[] arguments;
			string rawText;

			if (target == "SUB")
			{
				arguments = new[] { target, slot, subSlot, "追加攻撃力", "5" };
				rawText = $"EQUIP MITAMA SUB {slot} {subSlot} \"追加攻撃力\" \"5\"";
			}
			else
			{
				arguments = new[] { target, slot, "追加攻撃力" };
				rawText = $"EQUIP MITAMA {target} {slot} \"追加攻撃力\"";
			}

			var validator = new ScenarioValidator();
			var steps = new List<ScenarioStep>
			{
				new ScenarioStep(2, ScenarioCommandType.EQUIP_MITAMA, arguments, rawText)
			};
			var scenario = new Scenario("Test.scenario", 1, 3, steps);

			if (expectedValid)
			{
				validator.Validate(scenario);
				return;
			}

			int minimum = 1;
			int maximum = target == "SET" ? 3 : 6;

			if (target == "SUB" && int.TryParse(slot, out int mitamaSlot) && mitamaSlot >= 1 && mitamaSlot <= 6)
			{
				maximum = 4;
			}

			string expectedMessage = $"Argument must be an integer between {minimum} and {maximum} at line 2: {rawText}";

			try
			{
				validator.Validate(scenario);
				Assert.Fail("ScenarioValidationException was not thrown.");
			}
			catch (ScenarioValidationException ex)
			{
				Assert.AreEqual(expectedMessage, ex.Message);
			}
		}

		[TestMethod]
		public void Validate_EquipMitamaSubWithoutOptionalArguments()
		{
			var validator = new ScenarioValidator();
			var steps = new List<ScenarioStep>
			{
				new ScenarioStep(2, ScenarioCommandType.EQUIP_MITAMA, new[] { "SUB", "2", "1" }, "EQUIP MITAMA SUB 2 1")
			};
			var scenario = new Scenario("Test.scenario", 1, 3, steps);

			validator.Validate(scenario);
		}

		[TestMethod]
		public void Validate_EquipMitamaSubInvalidStatValue()
		{
			var validator = new ScenarioValidator();
			var steps = new List<ScenarioStep>
			{
				new ScenarioStep(2, ScenarioCommandType.EQUIP_MITAMA, new[] { "SUB", "2", "1", "追加攻撃力", "INVALID" }, "EQUIP MITAMA SUB 2 1 \"追加攻撃力\" \"INVALID\"")
			};
			var scenario = new Scenario("Test.scenario", 1, 3, steps);

			validator.Validate(scenario);
		}

		[TestMethod]
		public void Validate_EquipMitamaMissingArgument()
		{
			var validator = new ScenarioValidator();
			var steps = new List<ScenarioStep>
			{
				new ScenarioStep(2, ScenarioCommandType.EQUIP_MITAMA, new[] { "MAIN", "2" }, "EQUIP MITAMA MAIN 2")
			};
			var scenario = new Scenario("Test.scenario", 1, 3, steps);

			try
			{
				validator.Validate(scenario);
				Assert.Fail("ScenarioValidationException was not thrown.");
			}
			catch (ScenarioValidationException ex)
			{
				Assert.AreEqual("Argument is missing at line 2: EQUIP MITAMA MAIN 2", ex.Message);
			}
		}
	}
}
