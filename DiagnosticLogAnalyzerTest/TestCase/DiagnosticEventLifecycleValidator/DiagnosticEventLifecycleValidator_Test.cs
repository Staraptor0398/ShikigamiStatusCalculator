using DiagnosticLogAnalyzer.Model;
using DiagnosticLogAnalyzer.Validation;

namespace DiagnosticLogAnalyzerTest.TestCase
{
	[TestClass]
	public class DiagnosticEventLifecycleValidator_Test
	{
		private const string OperationCalculation = "Calculation";
		private const string OperationLoad = "SaveData.Load";
		private static readonly DateTime BaseTime = new DateTime(2026, 11, 20, 15, 27, 46);

		private static DiagnosticCommandCorrelation CreateEvent(string operation, string eventName, int millis,
			CommandRecord command = null, GuiWindowRecord window = null)
		{
			command = command ?? new CommandRecord { Type = "CALC", LineNumber = 10 };
			window = window ?? new GuiWindowRecord { WindowOrder = 1 };
			ScenarioRecord scenario = new ScenarioRecord { Scenario = "Test.scenario" };
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
						SessionId = "0123456789abcdef0123456789abcdef",
						SourceFile = "log.txt",
						LineNumber = millis,
						Timestamp = BaseTime.AddMilliseconds(millis),
						Operation = operation,
						EventName = eventName
					}
				}
			};
		}

		private static DiagnosticEventLifecycleValidationResult Validate(string operation, params (string name, int time)[] stages)
		{
			DiagnosticCommandCorrelationResult source = new DiagnosticCommandCorrelationResult();
			CommandRecord command = new CommandRecord { Type = "CALC", LineNumber = 10 };
			GuiWindowRecord window = new GuiWindowRecord { WindowOrder = 1 };
			ScenarioRecord scenario = new ScenarioRecord { Scenario = "Test.scenario" };
			foreach (var stage in stages)
			{
				var record = CreateEvent(operation, stage.name, stage.time, command, window);
				record.SessionCorrelation.Scenario = scenario;
				source.Events.Add(record);
			}
			return new DiagnosticEventLifecycleValidator().Validate(source);
		}

		private static DiagnosticLifecycleVerdict Verdict(string operation, params (string name, int time)[] stages)
		{
			return Validate(operation, stages).Lifecycles.Single().Verdict;
		}

		[TestMethod]
		public void CalculationStartedCompletedPasses()
		{
			Assert.AreEqual(DiagnosticLifecycleVerdict.Pass, Verdict(OperationCalculation, ("Started", 10), ("Completed", 20)));
		}

		[TestMethod]
		public void CalculationStartedFailedIsConsistent()
		{
			Assert.AreEqual(DiagnosticLifecycleVerdict.Pass, Verdict(OperationCalculation, ("Started", 10), ("Failed", 20)));
		}

		[TestMethod]
		public void CalculationValidationRejectedWithoutStartedPasses()
		{
			Assert.AreEqual(DiagnosticLifecycleVerdict.Pass, Verdict(OperationCalculation, ("ValidationRejected", 10)));
		}

		[TestMethod]
		public void CalculationMissingCompletedIsInconclusive()
		{
			Assert.AreEqual(DiagnosticLifecycleVerdict.Inconclusive, Verdict(OperationCalculation, ("Started", 10)));
		}

		[TestMethod]
		public void CalculationMissingStartedIsInconclusive()
		{
			Assert.AreEqual(DiagnosticLifecycleVerdict.Inconclusive, Verdict(OperationCalculation, ("Completed", 20)));
		}

		[TestMethod]
		public void CalculationTerminalBeforeStartedFails()
		{
			Assert.AreEqual(DiagnosticLifecycleVerdict.Fail, Verdict(OperationCalculation, ("Completed", 10), ("Started", 20)));
		}

		[TestMethod]
		public void CalculationSameMillisecondDoesNotFail()
		{
			Assert.AreEqual(DiagnosticLifecycleVerdict.Pass, Verdict(OperationCalculation, ("Started", 10), ("Completed", 10)));
		}

		[TestMethod]
		public void CalculationMixedTerminalOutcomesAreInconclusive()
		{
			Assert.AreEqual(DiagnosticLifecycleVerdict.Inconclusive, Verdict(OperationCalculation, ("Started", 10), ("Failed", 20), ("Completed", 30)));
		}

		[TestMethod]
		public void CalculationMixedRejectionAndStartIsInconclusive()
		{
			Assert.AreEqual(DiagnosticLifecycleVerdict.Inconclusive, Verdict(OperationCalculation, ("Started", 10), ("ValidationRejected", 20)));
		}

		[TestMethod]
		public void DuplicateStartedEventsAreInconclusive()
		{
			Assert.AreEqual(DiagnosticLifecycleVerdict.Inconclusive, Verdict(OperationCalculation, ("Started", 10), ("Started", 15), ("Completed", 20)));
		}

		[TestMethod]
		public void LoadValidatedCompletedPasses()
		{
			Assert.AreEqual(DiagnosticLifecycleVerdict.Pass, Verdict(OperationLoad, ("Started", 10), ("Validated", 20), ("Completed", 30)));
		}

		[TestMethod]
		public void LoadFailedBeforeValidationIsConsistent()
		{
			Assert.AreEqual(DiagnosticLifecycleVerdict.Pass, Verdict(OperationLoad, ("Started", 10), ("Failed", 20)));
		}

		[TestMethod]
		public void LoadFailedAfterValidationIsConsistent()
		{
			Assert.AreEqual(DiagnosticLifecycleVerdict.Pass, Verdict(OperationLoad, ("Started", 10), ("Validated", 20), ("Failed", 30)));
		}

		[TestMethod]
		public void LoadCompletedWithoutValidatedIsInconclusive()
		{
			Assert.AreEqual(DiagnosticLifecycleVerdict.Inconclusive, Verdict(OperationLoad, ("Started", 10), ("Completed", 30)));
		}

		[TestMethod]
		public void LoadValidatedBeforeStartedFails()
		{
			Assert.AreEqual(DiagnosticLifecycleVerdict.Fail, Verdict(OperationLoad, ("Validated", 10), ("Started", 20), ("Completed", 30)));
		}

		[TestMethod]
		public void LoadCompletedBeforeValidatedFails()
		{
			Assert.AreEqual(DiagnosticLifecycleVerdict.Fail, Verdict(OperationLoad, ("Started", 10), ("Completed", 20), ("Validated", 30)));
		}

		[TestMethod]
		public void UnmatchedEventsAreExcluded()
		{
			DiagnosticCommandCorrelationResult source = new DiagnosticCommandCorrelationResult();
			var record = CreateEvent(OperationCalculation, "Started", 10);
			record.Status = DiagnosticCommandMatchStatus.NoMatchingCommand;
			source.Events.Add(record);
			var result = new DiagnosticEventLifecycleValidator().Validate(source);
			Assert.AreEqual(0, result.Lifecycles.Count);
			Assert.AreEqual(1, result.ExcludedEvents);
		}

		[TestMethod]
		public void UnrelatedOperationIsExcluded()
		{
			DiagnosticCommandCorrelationResult source = new DiagnosticCommandCorrelationResult();
			source.Events.Add(CreateEvent("MainForm.State", "Changed", 10));
			Assert.AreEqual(0, new DiagnosticEventLifecycleValidator().Validate(source).Lifecycles.Count);
		}

		[TestMethod]
		public void DifferentWindowsAreSeparateLifecycles()
		{
			DiagnosticCommandCorrelationResult source = new DiagnosticCommandCorrelationResult();
			CommandRecord command = new CommandRecord { Type = "CALC" };
			source.Events.Add(CreateEvent(OperationCalculation, "Started", 10, command, new GuiWindowRecord { WindowOrder = 1 }));
			source.Events.Add(CreateEvent(OperationCalculation, "Completed", 20, command, new GuiWindowRecord { WindowOrder = 2 }));
			var result = new DiagnosticEventLifecycleValidator().Validate(source);
			Assert.AreEqual(2, result.Lifecycles.Count);
			Assert.IsTrue(result.Lifecycles.All(x => x.Verdict == DiagnosticLifecycleVerdict.Inconclusive));
		}

		[TestMethod]
		public void DifferentCommandsAreSeparateLifecycles()
		{
			DiagnosticCommandCorrelationResult source = new DiagnosticCommandCorrelationResult();
			GuiWindowRecord window = new GuiWindowRecord { WindowOrder = 1 };
			source.Events.Add(CreateEvent(OperationCalculation, "Started", 10, new CommandRecord { LineNumber = 1 }, window));
			source.Events.Add(CreateEvent(OperationCalculation, "Completed", 20, new CommandRecord { LineNumber = 2 }, window));
			Assert.AreEqual(2, new DiagnosticEventLifecycleValidator().Validate(source).Lifecycles.Count);
		}

		[TestMethod]
		public void InputOrderDoesNotChangeVerdict()
		{
			Assert.AreEqual(DiagnosticLifecycleVerdict.Pass, Verdict(OperationCalculation, ("Completed", 30), ("Started", 10)));
		}

		[TestMethod]
		public void NullResultIsRejected()
		{
			Assert.ThrowsExactly<ArgumentNullException>(() => new DiagnosticEventLifecycleValidator().Validate(null));
		}

		[TestMethod]
		public void NullEventIsRejected()
		{
			var source = new DiagnosticCommandCorrelationResult();
			source.Events.Add(null);
			Assert.ThrowsExactly<ArgumentException>(() => new DiagnosticEventLifecycleValidator().Validate(source));
		}
	}
}
