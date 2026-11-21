using DiagnosticLogAnalyzer.Model;
using DiagnosticLogAnalyzer.Validation;

namespace DiagnosticLogAnalyzerTest.TestCase
{
	[TestClass]
	public class SnapshotCompareModalContinuationValidator_Test
	{
		private const string SessionId = "0123456789abcdef0123456789abcdef";
		private static readonly DateTime BaseTime = new DateTime(2026, 11, 20, 15, 27, 46);

		private sealed class Fixture
		{
			public ScenarioRecord Scenario { get; } = new ScenarioRecord { Scenario = "Test.scenario" };
			public GuiWindowRecord Window { get; } = new GuiWindowRecord { WindowOrder = 1 };
			public CommandRecord Compare { get; } = new CommandRecord { LineNumber = 10, Type = "COMPARE SNAPSHOT", Result = "PASS" };
			public CommandRecord Check { get; } = new CommandRecord { LineNumber = 11, Type = "CHECK SNAPSHOT COMPARISON", Result = "PASS" };
			public CommandRecord Close { get; } = new CommandRecord { LineNumber = 12, Type = "CLOSE DIALOG", Result = "PASS" };
			public DiagnosticEventLifecycleValidationResult Lifecycles { get; } = new DiagnosticEventLifecycleValidationResult();
			public DiagnosticCommandCorrelationResult Correlations { get; } = new DiagnosticCommandCorrelationResult();
			public DiagnosticCommandExecutionContextResult Contexts { get; } = new DiagnosticCommandExecutionContextResult();
			public DiagnosticEventLifecycle Lifecycle { get; }

			public Fixture()
			{
				Scenario.Commands = new System.Collections.Generic.List<CommandRecord> { Compare, Check, Close };
				Lifecycle = new DiagnosticEventLifecycle
				{
					Operation = "Snapshot.Compare",
					Verdict = DiagnosticLifecycleVerdict.Inconclusive,
					Scenario = Scenario,
					Window = Window,
					Command = Compare
				};
				DiagnosticCommandCorrelation start = CreateCorrelation("Started", 100, DiagnosticCommandMatchStatus.Matched);
				start.Command = Compare;
				Lifecycle.Events.Add(start);
				Correlations.Events.Add(start);
				Lifecycles.Lifecycles.Add(Lifecycle);
				AddCompletion(300);
			}

			public DiagnosticCommandCorrelation CreateCorrelation(string name, int millis, DiagnosticCommandMatchStatus status,
				ScenarioRecord scenario = null, GuiWindowRecord window = null, string sessionId = SessionId)
			{
				return new DiagnosticCommandCorrelation
				{
					Status = status,
					SessionCorrelation = new DiagnosticEventCorrelation
					{
						Status = DiagnosticSessionMatchStatus.WindowMatched,
						Scenario = scenario ?? Scenario,
						Window = window ?? Window,
						Event = new DiagnosticEvent
						{
							SessionId = sessionId,
							Timestamp = BaseTime.AddMilliseconds(millis),
							Operation = "Snapshot.Compare",
							EventName = name
						}
					}
				};
			}

			public DiagnosticCommandExecutionContext AddCompletion(int millis, CommandRecord active = null,
					ScenarioRecord scenario = null, GuiWindowRecord window = null, string sessionId = SessionId)
			{
				DiagnosticCommandCorrelation record = CreateCorrelation("Completed", millis,
					DiagnosticCommandMatchStatus.NoMatchingCommand, scenario, window, sessionId);
				Correlations.Events.Add(record);
				DiagnosticCommandExecutionContext context = new DiagnosticCommandExecutionContext
				{
					Status = DiagnosticCommandContextStatus.ObservedDuringCommand,
					CommandCorrelation = record,
					ActiveCommand = active ?? Close
				};
				Contexts.Events.Add(context);
				return context;
			}

			public DiagnosticModalContinuation Evaluate()
			{
				return new SnapshotCompareModalContinuationValidator().Validate(Lifecycles, Correlations, Contexts).Items.Single();
			}
		}

		[TestMethod]
		public void ValidModalContinuationIsResolvedWithoutChangingOriginalCorrelation()
		{
			Fixture fixture = new Fixture();
			Assert.AreEqual(DiagnosticModalContinuationStatus.Resolved, fixture.Evaluate().Status);
			Assert.AreEqual(DiagnosticLifecycleVerdict.Inconclusive, fixture.Lifecycle.Verdict);
			Assert.AreEqual(DiagnosticCommandMatchStatus.NoMatchingCommand, fixture.Contexts.Events[0].CommandCorrelation.Status);
		}

		[TestMethod]
		public void SavedSnapshotComparisonIsSupported()
		{
			Fixture fixture = new Fixture();
			fixture.Compare.Type = "COMPARE SAVED SNAPSHOT";
			Assert.AreEqual(DiagnosticModalContinuationStatus.Resolved, fixture.Evaluate().Status);
		}

		[TestMethod]
		public void MissingComparisonCheckIsUnresolved()
		{
			Fixture fixture = new Fixture();
			fixture.Scenario.Commands.Remove(fixture.Check);
			Assert.AreEqual(DiagnosticModalContinuationStatus.Unresolved, fixture.Evaluate().Status);
		}

		[TestMethod]
		public void FailedComparisonCheckIsUnresolved()
		{
			Fixture fixture = new Fixture();
			fixture.Check.Result = "FAIL";
			Assert.AreEqual(DiagnosticModalContinuationStatus.Unresolved, fixture.Evaluate().Status);
		}

		[TestMethod]
		public void InterveningCommandPreventsContinuation()
		{
			Fixture fixture = new Fixture();
			fixture.Scenario.Commands.Insert(2, new CommandRecord { LineNumber = 11, Type = "OPEN CALC DETAIL", Result = "PASS" });
			Assert.AreEqual(DiagnosticModalContinuationStatus.Unresolved, fixture.Evaluate().Status);
		}

		[TestMethod]
		public void CompletedDuringAnotherCommandIsUnresolved()
		{
			Fixture fixture = new Fixture();
			fixture.Contexts.Events[0].ActiveCommand = fixture.Check;
			Assert.AreEqual(DiagnosticModalContinuationStatus.Unresolved, fixture.Evaluate().Status);
		}

		[TestMethod]
		public void DifferentGuiWindowIsNotLinked()
		{
			Fixture fixture = new Fixture();
			fixture.Contexts.Events[0].CommandCorrelation.SessionCorrelation.Window = new GuiWindowRecord { WindowOrder = 2 };
			Assert.AreEqual(DiagnosticModalContinuationStatus.Unresolved, fixture.Evaluate().Status);
		}

		[TestMethod]
		public void DifferentSessionIdIsNotLinked()
		{
			Fixture fixture = new Fixture();
			fixture.Contexts.Events[0].CommandCorrelation.SessionCorrelation.Event.SessionId = "ffffffffffffffffffffffffffffffff";
			Assert.AreEqual(DiagnosticModalContinuationStatus.Unresolved, fixture.Evaluate().Status);
		}

		[TestMethod]
		public void CompletionBeforeStartIsNotLinked()
		{
			Fixture fixture = new Fixture();
			fixture.Contexts.Events[0].CommandCorrelation.SessionCorrelation.Event.Timestamp = BaseTime.AddMilliseconds(50);
			Assert.AreEqual(DiagnosticModalContinuationStatus.Unresolved, fixture.Evaluate().Status);
		}

		[TestMethod]
		public void DuplicateCompletionIsAmbiguous()
		{
			Fixture fixture = new Fixture();
			fixture.AddCompletion(350);
			Assert.AreEqual(DiagnosticModalContinuationStatus.Ambiguous, fixture.Evaluate().Status);
		}

		[TestMethod]
		public void InterveningComparisonEventIsAmbiguous()
		{
			Fixture fixture = new Fixture();
			fixture.Correlations.Events.Add(fixture.CreateCorrelation("Started", 200, DiagnosticCommandMatchStatus.NoMatchingCommand));
			Assert.AreEqual(DiagnosticModalContinuationStatus.Ambiguous, fixture.Evaluate().Status);
		}

		[TestMethod]
		public void MultipleStartedEventsRemainUnresolved()
		{
			Fixture fixture = new Fixture();
			fixture.Lifecycle.Events.Add(fixture.CreateCorrelation("Started", 150, DiagnosticCommandMatchStatus.Matched));
			Assert.AreEqual(DiagnosticModalContinuationStatus.Unresolved, fixture.Evaluate().Status);
		}

		[TestMethod]
		public void AlreadyPassedLifecycleIsNotEvaluated()
		{
			Fixture fixture = new Fixture();
			fixture.Lifecycle.Verdict = DiagnosticLifecycleVerdict.Pass;
			Assert.AreEqual(0, new SnapshotCompareModalContinuationValidator().Validate(
				fixture.Lifecycles, fixture.Correlations, fixture.Contexts).Items.Count);
		}

		[TestMethod]
		public void FailedCloseDialogIsUnresolved()
		{
			Fixture fixture = new Fixture();
			fixture.Close.Result = "FAIL";
			Assert.AreEqual(DiagnosticModalContinuationStatus.Unresolved, fixture.Evaluate().Status);
		}

		[TestMethod]
		public void NullInputsAreRejected()
		{
			Fixture fixture = new Fixture();
			SnapshotCompareModalContinuationValidator validator = new SnapshotCompareModalContinuationValidator();
			Assert.ThrowsExactly<ArgumentNullException>(() => validator.Validate(null, fixture.Correlations, fixture.Contexts));
			Assert.ThrowsExactly<ArgumentNullException>(() => validator.Validate(fixture.Lifecycles, null, fixture.Contexts));
			Assert.ThrowsExactly<ArgumentNullException>(() => validator.Validate(fixture.Lifecycles, fixture.Correlations, null));
		}
	}
}