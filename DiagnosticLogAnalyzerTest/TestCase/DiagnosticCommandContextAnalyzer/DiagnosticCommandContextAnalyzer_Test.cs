using DiagnosticLogAnalyzer.Correlation;
using DiagnosticLogAnalyzer.Model;

namespace DiagnosticLogAnalyzerTest.TestCase
{
	[TestClass]
	public class DiagnosticCommandContextAnalyzer_Test
	{
		private static readonly DateTime Timestamp = new DateTime(2026, 11, 20, 15, 27, 46, 200);

		private static CommandRecord CreateCommand(string type, string start = "2026-11-20T15:27:46.100", string end = "2026-11-20T15:27:46.300")
		{
			return new CommandRecord { Type = type, Start = start, End = end, LineNumber = 12, Result = "PASS" };
		}

		private static DiagnosticCommandCorrelationResult CreateResult(DiagnosticCommandMatchStatus status, params CommandRecord[] commands)
		{
			ScenarioRecord scenario = new ScenarioRecord { Scenario = "Test.scenario", Commands = commands.ToList() };
			DiagnosticEventCorrelation session = new DiagnosticEventCorrelation
			{
				Status = DiagnosticSessionMatchStatus.WindowMatched,
				Event = new DiagnosticEvent { Timestamp = Timestamp },
				Scenario = scenario,
				Window = new GuiWindowRecord { WindowOrder = 1 }
			};
			DiagnosticCommandCorrelationResult result = new DiagnosticCommandCorrelationResult();
			result.Events.Add(new DiagnosticCommandCorrelation { SessionCorrelation = session, Status = status });
			return result;
		}

		[TestMethod]
		public void OneActiveCommandIsObservedWithoutChangingOriginalStatus()
		{
			CommandRecord command = CreateCommand("CHECK DIALOG");
			DiagnosticCommandCorrelationResult source = CreateResult(DiagnosticCommandMatchStatus.NoMatchingCommand, command);
			DiagnosticCommandExecutionContext context = new DiagnosticCommandContextAnalyzer().Analyze(source).Events.Single();
			Assert.AreEqual(DiagnosticCommandContextStatus.ObservedDuringCommand, context.Status);
			Assert.AreSame(command, context.ActiveCommand);
			Assert.AreSame(source.Events[0], context.CommandCorrelation);
			Assert.AreEqual(DiagnosticCommandMatchStatus.NoMatchingCommand, source.Events[0].Status);
		}

		[TestMethod]
		public void OutsideAllCommandsIsUnassigned()
		{
			var context = new DiagnosticCommandContextAnalyzer().Analyze(CreateResult(DiagnosticCommandMatchStatus.NoMatchingCommand,
				CreateCommand("CHECK DIALOG", "2026-11-20T15:27:45.000", "2026-11-20T15:27:45.999"))).Events.Single();
			Assert.AreEqual(DiagnosticCommandContextStatus.NoActiveCommand, context.Status);
			Assert.IsNull(context.ActiveCommand);
		}

		[TestMethod]
		public void OverlappingCommandsAreNotSelected()
		{
			var context = new DiagnosticCommandContextAnalyzer().Analyze(CreateResult(DiagnosticCommandMatchStatus.NoMatchingCommand,
				CreateCommand("CHECK DIALOG"), CreateCommand("CLOSE DIALOG"))).Events.Single();
			Assert.AreEqual(DiagnosticCommandContextStatus.MultipleActiveCommands, context.Status);
			Assert.IsNull(context.ActiveCommand);
			Assert.AreEqual(2, context.ActiveCommands.Count);
		}

		[TestMethod]
		public void MatchedCorrelationIsIgnored()
		{
			var result = new DiagnosticCommandContextAnalyzer().Analyze(CreateResult(DiagnosticCommandMatchStatus.Matched, CreateCommand("CHECK DIALOG")));
			Assert.AreEqual(0, result.Events.Count);
		}

		[TestMethod]
		public void NoConfirmedWindowIsIgnored()
		{
			var result = new DiagnosticCommandContextAnalyzer().Analyze(CreateResult(DiagnosticCommandMatchStatus.NoConfirmedWindow, CreateCommand("CHECK DIALOG")));
			Assert.AreEqual(0, result.Events.Count);
		}

		[TestMethod]
		public void InvalidTimestampIsIgnored()
		{
			var context = new DiagnosticCommandContextAnalyzer().Analyze(CreateResult(DiagnosticCommandMatchStatus.NoMatchingCommand,
				CreateCommand("CHECK DIALOG", "invalid", "2026-11-20T15:27:46.300"))).Events.Single();
			Assert.AreEqual(DiagnosticCommandContextStatus.NoActiveCommand, context.Status);
		}

		[TestMethod]
		public void ReversedIntervalIsIgnored()
		{
			var context = new DiagnosticCommandContextAnalyzer().Analyze(CreateResult(DiagnosticCommandMatchStatus.NoMatchingCommand,
				CreateCommand("CHECK DIALOG", "2026-11-20T15:27:46.300", "2026-11-20T15:27:46.100"))).Events.Single();
			Assert.AreEqual(DiagnosticCommandContextStatus.NoActiveCommand, context.Status);
		}

		[TestMethod]
		public void BothIntervalBoundariesAreIncluded()
		{
			var source = CreateResult(DiagnosticCommandMatchStatus.NoMatchingCommand, CreateCommand("CHECK DIALOG"));
			foreach (DateTime timestamp in new[] { new DateTime(2026, 11, 20, 15, 27, 46, 100), new DateTime(2026, 11, 20, 15, 27, 46, 300) })
			{
				source.Events[0].SessionCorrelation.Event.Timestamp = timestamp;
				Assert.AreEqual(DiagnosticCommandContextStatus.ObservedDuringCommand,
					new DiagnosticCommandContextAnalyzer().Analyze(source).Events.Single().Status);
			}
		}

		[TestMethod]
		public void EmptyCorrelationsProduceEmptyResult()
		{
			Assert.AreEqual(0, new DiagnosticCommandContextAnalyzer().Analyze(new DiagnosticCommandCorrelationResult()).Events.Count);
		}

		[TestMethod]
		public void NullCorrelationResultIsRejected()
		{
			Assert.ThrowsExactly<ArgumentNullException>(() => new DiagnosticCommandContextAnalyzer().Analyze(null));
		}

		[TestMethod]
		public void MissingConfirmedScenarioIsRejected()
		{
			var result = CreateResult(DiagnosticCommandMatchStatus.NoMatchingCommand, CreateCommand("CHECK DIALOG"));
			result.Events[0].SessionCorrelation.Scenario = null;
			Assert.ThrowsExactly<ArgumentException>(() => new DiagnosticCommandContextAnalyzer().Analyze(result));
		}

		[TestMethod]
		public void UnknownCommandEndDoesNotCountAsActive()
		{
			var context = new DiagnosticCommandContextAnalyzer().Analyze(CreateResult(DiagnosticCommandMatchStatus.NoMatchingCommand,
				CreateCommand("CHECK DIALOG", "2026-11-20T15:27:46.100", null))).Events.Single();
			Assert.AreEqual(DiagnosticCommandContextStatus.NoActiveCommand, context.Status);
		}
	}
}
