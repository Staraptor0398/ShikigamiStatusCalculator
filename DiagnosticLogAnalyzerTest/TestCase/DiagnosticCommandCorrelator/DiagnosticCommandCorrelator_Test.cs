using DiagnosticLogAnalyzer.Correlation;
using DiagnosticLogAnalyzer.Model;

namespace DiagnosticLogAnalyzerTest.TestCase
{
	[TestClass]
	public class DiagnosticCommandCorrelator_Test
	{
		private static readonly DateTime Timestamp = new DateTime(2026, 11, 20, 12, 0, 0, 100);

		private static DiagnosticEventCorrelation CreateEvent(string operation, string eventName = "Started")
		{
			return new DiagnosticEventCorrelation
			{
				Event = new DiagnosticEvent { Timestamp = Timestamp, Operation = operation, EventName = eventName },
				Status = DiagnosticSessionMatchStatus.WindowMatched,
				Scenario = new ScenarioRecord { Commands = new List<CommandRecord>() },
				Window = new GuiWindowRecord { WindowOrder = 1 }
			};
		}

		private static CommandRecord AddCommand(DiagnosticEventCorrelation item, string type,
			string start = "2026-11-20T12:00:00.000", string end = "2026-11-20T12:00:00.200")
		{
			CommandRecord command = new CommandRecord { Type = type, LineNumber = item.Scenario.Commands.Count + 1,
				Start = start, End = end };
			item.Scenario.Commands.Add(command);
			return command;
		}

		private static DiagnosticCommandCorrelation Correlate(DiagnosticEventCorrelation item)
		{
			DiagnosticSessionCorrelationResult input = new DiagnosticSessionCorrelationResult();
			input.Events.Add(item);
			return new DiagnosticCommandCorrelator().Correlate(input).Events[0];
		}

		[TestMethod]
		public void CalculationMatchesCalcCommand()
		{
			DiagnosticEventCorrelation item = CreateEvent("Calculation");
			CommandRecord command = AddCommand(item, "CALC");
			DiagnosticCommandCorrelation result = Correlate(item);
			Assert.AreEqual(DiagnosticCommandMatchStatus.Matched, result.Status);
			Assert.AreSame(command, result.Command);
			Assert.AreSame(item, result.SessionCorrelation);
		}

		[TestMethod]
		public void CalculationMatchesCalculateAlias()
		{
			DiagnosticEventCorrelation item = CreateEvent("Calculation");
			AddCommand(item, "CALCULATE");
			Assert.AreEqual(DiagnosticCommandMatchStatus.Matched, Correlate(item).Status);
		}

		[TestMethod]
		public void SaveDataLoadMatchesTryLoadBuild()
		{
			DiagnosticEventCorrelation item = CreateEvent("SaveData.Load", "Failed");
			AddCommand(item, "TRY LOAD BUILD");
			Assert.AreEqual(DiagnosticCommandMatchStatus.Matched, Correlate(item).Status);
		}

		[TestMethod]
		public void SaveDataSaveMatchesSaveSnapshot()
		{
			DiagnosticEventCorrelation item = CreateEvent("SaveData.Save");
			AddCommand(item, "SAVE SNAPSHOT");
			Assert.AreEqual(DiagnosticCommandMatchStatus.Matched, Correlate(item).Status);
		}

		[TestMethod]
		public void InitialReloadMatchesOpenGui()
		{
			DiagnosticEventCorrelation item = CreateEvent("Shikigami.Reload");
			AddCommand(item, "OPEN GUI");
			Assert.AreEqual(DiagnosticCommandMatchStatus.Matched, Correlate(item).Status);
		}

		[TestMethod]
		public void ApplicationClosingMatchesCloseGui()
		{
			DiagnosticEventCorrelation item = CreateEvent("Application", "Closing");
			AddCommand(item, "CLOSE GUI");
			Assert.AreEqual(DiagnosticCommandMatchStatus.Matched, Correlate(item).Status);
		}

		[TestMethod]
		public void ApplicationStartedDoesNotMatchCloseGui()
		{
			DiagnosticEventCorrelation item = CreateEvent("Application", "Started");
			AddCommand(item, "CLOSE GUI");
			Assert.AreEqual(DiagnosticCommandMatchStatus.NoMatchingCommand, Correlate(item).Status);
		}

		[TestMethod]
		public void GenericStateChangeIsNotMapped()
		{
			DiagnosticEventCorrelation item = CreateEvent("MainForm.State", "Changed");
			AddCommand(item, "CALC");
			Assert.AreEqual(DiagnosticCommandMatchStatus.NotMappedOperation, Correlate(item).Status);
		}

		[TestMethod]
		public void WrongOperationCommandCombinationDoesNotMatch()
		{
			DiagnosticEventCorrelation item = CreateEvent("SaveData.Load");
			AddCommand(item, "CALC");
			Assert.AreEqual(DiagnosticCommandMatchStatus.NoMatchingCommand, Correlate(item).Status);
		}

		[TestMethod]
		public void OutsideCommandIntervalDoesNotMatch()
		{
			DiagnosticEventCorrelation item = CreateEvent("Calculation");
			AddCommand(item, "CALC", "2026-11-20T12:00:00.200", "2026-11-20T12:00:00.300");
			Assert.AreEqual(DiagnosticCommandMatchStatus.NoMatchingCommand, Correlate(item).Status);
		}

		[TestMethod]
		public void OverlappingCommandsRemainAmbiguous()
		{
			DiagnosticEventCorrelation item = CreateEvent("Calculation");
			AddCommand(item, "CALC");
			AddCommand(item, "CALCULATE");
			DiagnosticCommandCorrelation result = Correlate(item);
			Assert.AreEqual(DiagnosticCommandMatchStatus.Ambiguous, result.Status);
			Assert.IsNull(result.Command);
			Assert.AreEqual(2, result.Candidates.Count);
		}

		[TestMethod]
		public void UnmatchedSessionNeverReceivesConfirmedCommand()
		{
			DiagnosticEventCorrelation item = CreateEvent("Calculation");
			item.Status = DiagnosticSessionMatchStatus.Unmatched;
			item.Scenario = null;
			item.Window = null;
			item.WindowCandidates.Add(new DiagnosticWindowCandidate { Scenario = new ScenarioRecord(), Window = new GuiWindowRecord() });
			Assert.AreEqual(DiagnosticCommandMatchStatus.NoConfirmedWindow, Correlate(item).Status);
		}

		[TestMethod]
		public void PerformanceOnlyCannotDetermineCommand()
		{
			DiagnosticEventCorrelation item = CreateEvent("Calculation");
			item.Status = DiagnosticSessionMatchStatus.PerformanceOnly;
			Assert.AreEqual(DiagnosticCommandMatchStatus.NoConfirmedWindow, Correlate(item).Status);
		}

		[TestMethod]
		public void InvalidCommandTimestampIsIgnored()
		{
			DiagnosticEventCorrelation item = CreateEvent("Calculation");
			AddCommand(item, "CALC", "invalid");
			Assert.AreEqual(DiagnosticCommandMatchStatus.NoMatchingCommand, Correlate(item).Status);
		}

		[TestMethod]
		public void CommandWithNullEndIsNotMatched()
		{
			DiagnosticEventCorrelation item = CreateEvent("Calculation");
			AddCommand(item, "CALC", end: null);
			Assert.AreEqual(DiagnosticCommandMatchStatus.NoMatchingCommand, Correlate(item).Status);
		}

		[TestMethod]
		public void EmptySessionEventsProduceEmptyResult()
		{
			Assert.AreEqual(0, new DiagnosticCommandCorrelator().Correlate(new DiagnosticSessionCorrelationResult()).Events.Count);
		}

		[TestMethod]
		public void NullInputIsRejected()
		{
			Assert.ThrowsExactly<ArgumentNullException>(() => new DiagnosticCommandCorrelator().Correlate(null));
		}

		[TestMethod]
		public void NullEventIsRejected()
		{
			DiagnosticSessionCorrelationResult sessions = new DiagnosticSessionCorrelationResult();
			sessions.Events.Add(null);
			Assert.ThrowsExactly<ArgumentException>(() => new DiagnosticCommandCorrelator().Correlate(sessions));
		}
	}
}