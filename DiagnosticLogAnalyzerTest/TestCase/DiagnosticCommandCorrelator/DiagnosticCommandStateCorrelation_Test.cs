using DiagnosticLogAnalyzer.Correlation;
using DiagnosticLogAnalyzer.Model;

namespace DiagnosticLogAnalyzerTest.TestCase
{
	[TestClass]
	public class DiagnosticCommandStateCorrelation_Test
	{
		private const string SessionId = "0123456789abcdef0123456789abcdef";
		private const string ScenarioFile = "ScenarioRunnerCli/Log/State.scenario.log";
		private const string Start = "2026-11-20T15:27:46.100";
		private const string End = "2026-11-20T15:27:46.400";

		private static AnalysisInput createInput(string commandType)
		{
			return new AnalysisInput
			{
				Scenarios = new List<ScenarioRecord>
				{
					new ScenarioRecord
					{
						Scenario = "State.scenario",
						SourceFile = ScenarioFile,
						Commands = new List<CommandRecord>
						{
							new CommandRecord { LineNumber = 10, Type = commandType, Start = Start, End = End }
						},
						GuiWindows = new List<GuiWindowRecord>
						{
							new GuiWindowRecord { WindowOrder = 1, SessionId = SessionId }
						}
					}
				},
				PerformanceSessions = new List<PerformanceSessionRecord>
				{
					new PerformanceSessionRecord { SessionId = SessionId, ScenarioFile = ScenarioFile, WindowOrder = 1 }
				}
			};
		}

		private static DiagnosticCommandCorrelation correlate(AnalysisInput input, string operation, string eventName,
			string details, int millisecond = 200)
		{
			DiagnosticEvent diagnosticEvent = new DiagnosticEvent
			{
				SessionId = SessionId,
				Timestamp = new DateTime(2026, 11, 20, 15, 27, 46, millisecond),
				Operation = operation,
				EventName = eventName,
				Details = details
			};
			DiagnosticSessionCorrelationResult sessions = new DiagnosticSessionCorrelator().Correlate(input,
				new[] { diagnosticEvent });
			return new DiagnosticCommandCorrelator().Correlate(sessions).Events[0];
		}

		private static void assertMatched(DiagnosticCommandCorrelation result, AnalysisInput input)
		{
			Assert.AreEqual(DiagnosticCommandMatchStatus.Matched, result.Status);
			Assert.AreSame(input.Scenarios[0].Commands[0], result.Command);
			Assert.AreEqual(1, result.Candidates.Count);
		}

		[TestMethod]
		public void LoadingStartMatchesLoadCommand()
		{
			AnalysisInput input = createInput("LOAD MITAMA");
			assertMatched(correlate(input, "MainForm.State", "Changed", "Previous=IDLE Current=LOADING"), input);
		}

		[TestMethod]
		public void LoadingEndMatchesTryLoadCommand()
		{
			AnalysisInput input = createInput("TRY LOAD BUILD");
			assertMatched(correlate(input, "MainForm.State", "Changed", "Previous=LOADING Current=IDLE"), input);
		}

		[TestMethod]
		public void SavingStartMatchesSaveCommand()
		{
			AnalysisInput input = createInput("SAVE BUILD");
			assertMatched(correlate(input, "MainForm.State", "Changed", "Previous=IDLE Current=SAVING"), input);
		}

		[TestMethod]
		public void SavingEndMatchesSnapshotSaveCommand()
		{
			AnalysisInput input = createInput("SAVE SNAPSHOT");
			assertMatched(correlate(input, "MainForm.State", "Changed", "Previous=SAVING Current=IDLE"), input);
		}

		[TestMethod]
		public void ClearingStartMatchesClearCommand()
		{
			AnalysisInput input = createInput("CLEAR");
			assertMatched(correlate(input, "MainForm.State", "Changed", "Previous=IDLE Current=CLEARING"), input);
		}

		[TestMethod]
		public void ClearingEndMatchesClearShikigamiCommand()
		{
			AnalysisInput input = createInput("CLEAR SHIKIGAMI");
			assertMatched(correlate(input, "MainForm.State", "Changed", "Previous=CLEARING Current=IDLE"), input);
		}

		[TestMethod]
		public void DirtyChangedMatchesEquipMitamaCommand()
		{
			AnalysisInput input = createInput("EQUIP MITAMA");
			assertMatched(correlate(input, "Calculation.Result", "DirtyChanged", "Current=Dirty"), input);
		}

		[TestMethod]
		public void DirtyChangedMatchesClearCommand()
		{
			AnalysisInput input = createInput("CLEAR");
			assertMatched(correlate(input, "Calculation.Result", "DirtyChanged", "Current=Dirty"), input);
		}

		[TestMethod]
		public void DirtyChangedMatchesClearShikigamiCommand()
		{
			AnalysisInput input = createInput("CLEAR SHIKIGAMI");
			assertMatched(correlate(input, "Calculation.Result", "DirtyChanged", "Current=Dirty"), input);
		}

		[TestMethod]
		public void ClearingDuringCheckClearedIsNotGuessed()
		{
			AnalysisInput input = createInput("CHECK CLEARED");
			DiagnosticCommandCorrelation result = correlate(input, "MainForm.State", "Changed", "Previous=IDLE Current=CLEARING");
			Assert.AreEqual(DiagnosticCommandMatchStatus.NoMatchingCommand, result.Status);
			Assert.IsNull(result.Command);
		}

		[TestMethod]
		public void ClearingDuringConfirmDialogIsNotGuessed()
		{
			AnalysisInput input = createInput("CONFIRM DIALOG");
			DiagnosticCommandCorrelation result = correlate(input, "MainForm.State", "Changed", "Previous=CLEARING Current=IDLE");
			Assert.AreEqual(DiagnosticCommandMatchStatus.NoMatchingCommand, result.Status);
		}

		[TestMethod]
		public void DirtyChangedDuringCheckIsNotGuessed()
		{
			AnalysisInput input = createInput("CHECK CLEARED");
			DiagnosticCommandCorrelation result = correlate(input, "Calculation.Result", "DirtyChanged", "Current=Dirty");
			Assert.AreEqual(DiagnosticCommandMatchStatus.NoMatchingCommand, result.Status);
		}

		[TestMethod]
		public void UnknownStateTransitionIsNotMapped()
		{
			AnalysisInput input = createInput("LOAD MITAMA");
			DiagnosticCommandCorrelation result = correlate(input, "MainForm.State", "Changed", "Previous=IDLE Current=UNKNOWN");
			Assert.AreEqual(DiagnosticCommandMatchStatus.NotMappedOperation, result.Status);
		}

		[TestMethod]
		public void StateChangedWithoutPreviousValueIsNotMapped()
		{
			AnalysisInput input = createInput("LOAD MITAMA");
			DiagnosticCommandCorrelation result = correlate(input, "MainForm.State", "Changed", "Current=LOADING");
			Assert.AreEqual(DiagnosticCommandMatchStatus.NotMappedOperation, result.Status);
		}

		[TestMethod]
		public void WrongStateEventNameIsNotMapped()
		{
			AnalysisInput input = createInput("LOAD MITAMA");
			DiagnosticCommandCorrelation result = correlate(input, "MainForm.State", "Other", "Previous=IDLE Current=LOADING");
			Assert.AreEqual(DiagnosticCommandMatchStatus.NotMappedOperation, result.Status);
		}

		[TestMethod]
		public void WrongDirtyDetailsAreNotMapped()
		{
			AnalysisInput input = createInput("EQUIP MITAMA");
			DiagnosticCommandCorrelation result = correlate(input, "Calculation.Result", "DirtyChanged", "Current=Clean");
			Assert.AreEqual(DiagnosticCommandMatchStatus.NotMappedOperation, result.Status);
		}

		[TestMethod]
		public void StateCommandRequiresMatchingTime()
		{
			AnalysisInput input = createInput("LOAD MITAMA");
			DiagnosticCommandCorrelation result = correlate(input, "MainForm.State", "Changed", "Previous=IDLE Current=LOADING", 500);
			Assert.AreEqual(DiagnosticCommandMatchStatus.NoMatchingCommand, result.Status);
		}

		[TestMethod]
		public void StateCommandRequiresConfirmedSession()
		{
			AnalysisInput input = createInput("LOAD MITAMA");
			input.Scenarios[0].GuiWindows[0].SessionId = null;
			DiagnosticCommandCorrelation result = correlate(input, "MainForm.State", "Changed", "Previous=IDLE Current=LOADING");
			Assert.AreEqual(DiagnosticCommandMatchStatus.NoConfirmedWindow, result.Status);
		}

		[TestMethod]
		public void MultipleOverlappingCommandsRemainAmbiguous()
		{
			AnalysisInput input = createInput("LOAD MITAMA");
			input.Scenarios[0].Commands.Add(new CommandRecord { LineNumber = 11, Type = "TRY LOAD MITAMA", Start = Start, End = End });
			DiagnosticCommandCorrelation result = correlate(input, "MainForm.State", "Changed", "Previous=LOADING Current=IDLE");
			Assert.AreEqual(DiagnosticCommandMatchStatus.Ambiguous, result.Status);
			Assert.IsNull(result.Command);
			Assert.AreEqual(2, result.Candidates.Count);
		}

		[TestMethod]
		public void ExistingCalculationMappingRemainsUnchanged()
		{
			AnalysisInput input = createInput("CALC");
			assertMatched(correlate(input, "Calculation", "Started", "ShikigamiSelected=True"), input);
		}
	}
}
