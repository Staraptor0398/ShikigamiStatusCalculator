using DiagnosticLogAnalyzer.Correlation;
using DiagnosticLogAnalyzer.Model;

namespace DiagnosticLogAnalyzerTest.TestCase
{
	[TestClass]
	public class DiagnosticSessionCorrelator_Test
	{
		private const string SessionA = "0123456789abcdef0123456789abcdef";
		private const string SessionB = "abcdef0123456789abcdef0123456789";
		private const string ScenarioFile = "ScenarioRunnerCli/Log/Test.scenario.log";

		private static AnalysisInput CreateInput()
		{
			return new AnalysisInput
			{
				Scenarios = new List<ScenarioRecord>
				{
					new ScenarioRecord
					{
						Scenario = "Test.scenario",
						SourceFile = ScenarioFile,
						GuiWindows = new List<GuiWindowRecord>
						{
							new GuiWindowRecord { WindowOrder = 1, SessionId = SessionA, MatchQuality = "window-time" }
						}
					}
				},
				PerformanceSessions = new List<PerformanceSessionRecord>
				{
					new PerformanceSessionRecord { SessionId = SessionA, ScenarioFile = ScenarioFile, WindowOrder = 1 }
				}
			};
		}

		private static DiagnosticEvent CreateEvent(string sessionId)
		{
			return new DiagnosticEvent { SessionId = sessionId, SourceFile = "events.log", LineNumber = 1 };
		}

		[TestMethod]
		public void MatchedWindowAndPerformanceLinkScenario()
		{
			AnalysisInput input = CreateInput();
			DiagnosticSessionCorrelationResult result = new DiagnosticSessionCorrelator().Correlate(input, new[] { CreateEvent(SessionA) });
			Assert.AreEqual(1, result.Events.Count);
			Assert.AreEqual(DiagnosticSessionMatchStatus.WindowMatched, result.Events[0].Status);
			Assert.AreSame(input.Scenarios[0], result.Events[0].Scenario);
			Assert.AreSame(input.Scenarios[0].GuiWindows[0], result.Events[0].Window);
			Assert.AreSame(input.PerformanceSessions[0], result.Events[0].PerformanceSession);
			Assert.AreEqual(0, result.Issues.Count);
		}

		[TestMethod]
		public void WindowWithoutPerformanceStillLinksScenario()
		{
			AnalysisInput input = CreateInput();
			input.PerformanceSessions.Clear();
			DiagnosticEventCorrelation item = new DiagnosticSessionCorrelator().Correlate(input, new[] { CreateEvent(SessionA) }).Events[0];
			Assert.AreEqual(DiagnosticSessionMatchStatus.WindowMatched, item.Status);
			Assert.AreSame(input.Scenarios[0], item.Scenario);
			Assert.IsNull(item.PerformanceSession);
		}

		[TestMethod]
		public void PerformanceWithoutWindowDoesNotGuessScenario()
		{
			AnalysisInput input = CreateInput();
			input.Scenarios[0].GuiWindows.Clear();
			DiagnosticEventCorrelation item = new DiagnosticSessionCorrelator().Correlate(input, new[] { CreateEvent(SessionA) }).Events[0];
			Assert.AreEqual(DiagnosticSessionMatchStatus.PerformanceOnly, item.Status);
			Assert.AreSame(input.PerformanceSessions[0], item.PerformanceSession);
			Assert.IsNull(item.Scenario);
			Assert.IsNull(item.Window);
		}

		[TestMethod]
		public void UnknownSessionRemainsUnmatched()
		{
			DiagnosticEventCorrelation item = new DiagnosticSessionCorrelator().Correlate(CreateInput(), new[] { CreateEvent(SessionB) }).Events[0];
			Assert.AreEqual(DiagnosticSessionMatchStatus.Unmatched, item.Status);
			Assert.IsNull(item.Scenario);
			Assert.IsNull(item.PerformanceSession);
		}

		[TestMethod]
		public void SessionIdMatchingIsCaseInsensitive()
		{
			DiagnosticEventCorrelation item = new DiagnosticSessionCorrelator().Correlate(CreateInput(), new[] { CreateEvent(SessionA.ToUpperInvariant()) }).Events[0];
			Assert.AreEqual(DiagnosticSessionMatchStatus.WindowMatched, item.Status);
		}

		[TestMethod]
		public void DuplicateWindowsAreAmbiguous()
		{
			AnalysisInput input = CreateInput();
			input.Scenarios[0].GuiWindows.Add(new GuiWindowRecord { WindowOrder = 2, SessionId = SessionA });
			DiagnosticSessionCorrelationResult result = new DiagnosticSessionCorrelator().Correlate(input, new[] { CreateEvent(SessionA) });
			Assert.AreEqual(DiagnosticSessionMatchStatus.Ambiguous, result.Events[0].Status);
			Assert.IsNull(result.Events[0].Scenario);
			Assert.AreEqual(1, result.Issues.Count);
		}

		[TestMethod]
		public void DuplicatePerformanceSessionsAreAmbiguous()
		{
			AnalysisInput input = CreateInput();
			input.PerformanceSessions.Add(new PerformanceSessionRecord { SessionId = SessionA });
			DiagnosticSessionCorrelationResult result = new DiagnosticSessionCorrelator().Correlate(input, new[] { CreateEvent(SessionA) });
			Assert.AreEqual(DiagnosticSessionMatchStatus.Ambiguous, result.Events[0].Status);
			Assert.AreEqual(1, result.Issues.Count);
		}

		[TestMethod]
		public void ConflictingWindowOrderIsAmbiguous()
		{
			AnalysisInput input = CreateInput();
			input.PerformanceSessions[0].WindowOrder = 2;
			DiagnosticEventCorrelation item = new DiagnosticSessionCorrelator().Correlate(input, new[] { CreateEvent(SessionA) }).Events[0];
			Assert.AreEqual(DiagnosticSessionMatchStatus.Ambiguous, item.Status);
			Assert.IsNull(item.Scenario);
		}

		[TestMethod]
		public void ConflictingScenarioPathIsAmbiguous()
		{
			AnalysisInput input = CreateInput();
			input.PerformanceSessions[0].ScenarioFile = "ScenarioRunnerCli/Log/Other.scenario.log";
			DiagnosticEventCorrelation item = new DiagnosticSessionCorrelator().Correlate(input, new[] { CreateEvent(SessionA) }).Events[0];
			Assert.AreEqual(DiagnosticSessionMatchStatus.Ambiguous, item.Status);
		}

		[TestMethod]
		public void UnmatchedWindowsWithNoSessionIdAreNotAssigned()
		{
			AnalysisInput input = CreateInput();
			input.Scenarios[0].GuiWindows[0].SessionId = null;
			DiagnosticEventCorrelation item = new DiagnosticSessionCorrelator().Correlate(input, new[] { CreateEvent(SessionA) }).Events[0];
			Assert.AreEqual(DiagnosticSessionMatchStatus.PerformanceOnly, item.Status);
			Assert.IsNull(item.Window);
		}

		[TestMethod]
		public void AllEventsRetainInputOrderAndSource()
		{
			DiagnosticEvent a = CreateEvent(SessionB);
			DiagnosticEvent b = CreateEvent(SessionA);
			DiagnosticSessionCorrelationResult result = new DiagnosticSessionCorrelator().Correlate(CreateInput(), new[] { a, b });
			Assert.AreEqual(2, result.Events.Count);
			Assert.AreSame(a, result.Events[0].Event);
			Assert.AreSame(b, result.Events[1].Event);
			Assert.AreEqual(DiagnosticSessionMatchStatus.Unmatched, result.Events[0].Status);
			Assert.AreEqual(DiagnosticSessionMatchStatus.WindowMatched, result.Events[1].Status);
		}

		[TestMethod]
		public void AmbiguityIssueIsGroupedBySessionId()
		{
			AnalysisInput input = CreateInput();
			input.PerformanceSessions.Add(new PerformanceSessionRecord { SessionId = SessionA });
			DiagnosticSessionCorrelationResult result = new DiagnosticSessionCorrelator().Correlate(input, new[] { CreateEvent(SessionA), CreateEvent(SessionA) });
			Assert.AreEqual(2, result.Events.Count);
			Assert.AreEqual(1, result.Issues.Count);
			StringAssert.Contains(result.Issues[0], "2 events");
		}

		[TestMethod]
		public void EmptyEventsProducesEmptyResult()
		{
			DiagnosticSessionCorrelationResult result = new DiagnosticSessionCorrelator().Correlate(CreateInput(), Array.Empty<DiagnosticEvent>());
			Assert.AreEqual(0, result.Events.Count);
			Assert.AreEqual(0, result.Issues.Count);
		}

		[TestMethod]
		public void NullInputIsRejected()
		{
			Assert.ThrowsExactly<ArgumentNullException>(() => new DiagnosticSessionCorrelator().Correlate(null, new DiagnosticEvent[0]));
		}

		[TestMethod]
		public void NullEventsAreRejected()
		{
			Assert.ThrowsExactly<ArgumentNullException>(() => new DiagnosticSessionCorrelator().Correlate(CreateInput(), null));
		}
	}
}
