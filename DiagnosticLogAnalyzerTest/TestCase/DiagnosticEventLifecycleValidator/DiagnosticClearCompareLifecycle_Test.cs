using DiagnosticLogAnalyzer.Model;
using DiagnosticLogAnalyzer.Validation;
using System;
using System.Linq;

namespace DiagnosticLogAnalyzerTest.TestCase
{
	[TestClass]
	public class DiagnosticClearCompareLifecycle_Test
	{
		private static readonly DateTime BaseTime = new DateTime(2026, 11, 20, 15, 27, 46);

		private static DiagnosticCommandCorrelation CreateEvent(string operation, string eventName, int milliseconds,
			ScenarioRecord scenario, GuiWindowRecord window, CommandRecord command)
		{
			return new DiagnosticCommandCorrelation
			{
				Status = DiagnosticCommandMatchStatus.Matched,
				Command = command,
				SessionCorrelation = new DiagnosticEventCorrelation
				{
					Status = DiagnosticSessionMatchStatus.WindowMatched,
					Scenario = scenario,
					Window = window,
					Event = new DiagnosticEvent
					{
						SourceFile = "Test.log",
						LineNumber = milliseconds + 1,
						SessionId = "0123456789abcdef0123456789abcdef",
						Timestamp = BaseTime.AddMilliseconds(milliseconds),
						Operation = operation,
						EventName = eventName
					}
				}
			};
		}

		private static DiagnosticLifecycleVerdict Verdict(string operation, params (string name, int time)[] stages)
		{
			DiagnosticCommandCorrelationResult correlations = new DiagnosticCommandCorrelationResult();
			ScenarioRecord scenario = new ScenarioRecord { Scenario = "Test.scenario" };
			GuiWindowRecord window = new GuiWindowRecord { WindowOrder = 1 };
			CommandRecord command = new CommandRecord { Type = "CLEAR", LineNumber = 10 };
			foreach (var stage in stages)
			{
				correlations.Events.Add(CreateEvent(operation, stage.name, stage.time, scenario, window, command));
			}
			return new DiagnosticEventLifecycleValidator().Validate(correlations).Lifecycles.Single().Verdict;
		}

		[TestMethod]
		public void ClearStartedCompletedPasses()
		{
			Assert.AreEqual(DiagnosticLifecycleVerdict.Pass, Verdict("MainForm.Clear", ("Started", 10), ("Completed", 20)));
		}

		[TestMethod]
		public void ClearStartedFailedPasses()
		{
			Assert.AreEqual(DiagnosticLifecycleVerdict.Pass, Verdict("MainForm.Clear", ("Started", 10), ("Failed", 20)));
		}

		[TestMethod]
		public void ClearMissingStartedIsInconclusive()
		{
			Assert.AreEqual(DiagnosticLifecycleVerdict.Inconclusive, Verdict("MainForm.Clear", ("Completed", 20)));
		}

		[TestMethod]
		public void ClearMissingTerminalIsInconclusive()
		{
			Assert.AreEqual(DiagnosticLifecycleVerdict.Inconclusive, Verdict("MainForm.Clear", ("Started", 10)));
		}

		[TestMethod]
		public void ClearTerminalBeforeStartedFails()
		{
			Assert.AreEqual(DiagnosticLifecycleVerdict.Fail, Verdict("MainForm.Clear", ("Completed", 10), ("Started", 20)));
		}

		[TestMethod]
		public void ClearSameMillisecondPasses()
		{
			Assert.AreEqual(DiagnosticLifecycleVerdict.Pass, Verdict("MainForm.Clear", ("Started", 10), ("Completed", 10)));
		}

		[TestMethod]
		public void ClearUnexpectedValidatedIsInconclusive()
		{
			Assert.AreEqual(DiagnosticLifecycleVerdict.Inconclusive, Verdict("MainForm.Clear", ("Started", 10), ("Validated", 15), ("Completed", 20)));
		}

		[TestMethod]
		public void ClearMixedTerminalOutcomesAreInconclusive()
		{
			Assert.AreEqual(DiagnosticLifecycleVerdict.Inconclusive, Verdict("MainForm.Clear", ("Started", 10), ("Failed", 20), ("Completed", 30)));
		}

		[TestMethod]
		public void CompareStartedCompletedPasses()
		{
			Assert.AreEqual(DiagnosticLifecycleVerdict.Pass, Verdict("Snapshot.Compare", ("Started", 10), ("Completed", 20)));
		}

		[TestMethod]
		public void CompareStartedFailedPasses()
		{
			Assert.AreEqual(DiagnosticLifecycleVerdict.Pass, Verdict("Snapshot.Compare", ("Started", 10), ("Failed", 20)));
		}

		[TestMethod]
		public void CompareMissingStartedIsInconclusive()
		{
			Assert.AreEqual(DiagnosticLifecycleVerdict.Inconclusive, Verdict("Snapshot.Compare", ("Completed", 20)));
		}

		[TestMethod]
		public void CompareMissingTerminalIsInconclusive()
		{
			Assert.AreEqual(DiagnosticLifecycleVerdict.Inconclusive, Verdict("Snapshot.Compare", ("Started", 10)));
		}

		[TestMethod]
		public void CompareTerminalBeforeStartedFails()
		{
			Assert.AreEqual(DiagnosticLifecycleVerdict.Fail, Verdict("Snapshot.Compare", ("Failed", 10), ("Started", 20)));
		}

		[TestMethod]
		public void CompareDuplicateStartedIsInconclusive()
		{
			Assert.AreEqual(DiagnosticLifecycleVerdict.Inconclusive, Verdict("Snapshot.Compare", ("Started", 10), ("Started", 15), ("Completed", 20)));
		}

		[TestMethod]
		public void CompareValidationRejectedIsInconclusive()
		{
			Assert.AreEqual(DiagnosticLifecycleVerdict.Inconclusive, Verdict("Snapshot.Compare", ("Started", 10), ("ValidationRejected", 15), ("Completed", 20)));
		}

		[TestMethod]
		public void CompareUnexpectedEventIsInconclusive()
		{
			Assert.AreEqual(DiagnosticLifecycleVerdict.Inconclusive, Verdict("Snapshot.Compare", ("Started", 10), ("UnknownEvent", 15), ("Completed", 20)));
		}

		[TestMethod]
		public void CompareUnmatchedCompletedDoesNotBecomeConfirmed()
		{
			DiagnosticCommandCorrelationResult correlations = new DiagnosticCommandCorrelationResult();
			ScenarioRecord scenario = new ScenarioRecord { Scenario = "Test.scenario" };
			GuiWindowRecord window = new GuiWindowRecord { WindowOrder = 1 };
			CommandRecord command = new CommandRecord { Type = "COMPARE SNAPSHOT", LineNumber = 10 };
			correlations.Events.Add(CreateEvent("Snapshot.Compare", "Started", 10, scenario, window, command));
			DiagnosticCommandCorrelation completed = CreateEvent("Snapshot.Compare", "Completed", 20, scenario, window, command);
			completed.Status = DiagnosticCommandMatchStatus.NoMatchingCommand;
			completed.Command = null;
			correlations.Events.Add(completed);
			DiagnosticEventLifecycleValidationResult result = new DiagnosticEventLifecycleValidator().Validate(correlations);
			Assert.AreEqual(1, result.Lifecycles.Count);
			Assert.AreEqual(DiagnosticLifecycleVerdict.Inconclusive, result.Lifecycles[0].Verdict);
			Assert.AreEqual(1, result.ExcludedEvents);
		}

		[TestMethod]
		public void ClearDifferentCommandsRemainSeparate()
		{
			DiagnosticCommandCorrelationResult correlations = new DiagnosticCommandCorrelationResult();
			ScenarioRecord scenario = new ScenarioRecord { Scenario = "Test.scenario" };
			GuiWindowRecord window = new GuiWindowRecord { WindowOrder = 1 };
			correlations.Events.Add(CreateEvent("MainForm.Clear", "Started", 10, scenario, window,
				new CommandRecord { Type = "CLEAR", LineNumber = 10 }));
			correlations.Events.Add(CreateEvent("MainForm.Clear", "Completed", 20, scenario, window,
				new CommandRecord { Type = "CHECK CLEARED", LineNumber = 11 }));
			DiagnosticEventLifecycleValidationResult result = new DiagnosticEventLifecycleValidator().Validate(correlations);
			Assert.AreEqual(2, result.Lifecycles.Count);
			Assert.IsTrue(result.Lifecycles.All(x => x.Verdict == DiagnosticLifecycleVerdict.Inconclusive));
		}
	}
}
