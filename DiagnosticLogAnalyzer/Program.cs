using DiagnosticLogAnalyzer.Correlation;
using DiagnosticLogAnalyzer.Model;
using DiagnosticLogAnalyzer.Parser;
using DiagnosticLogAnalyzer.Reader;
using DiagnosticLogAnalyzer.Validation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DiagnosticLogAnalyzer
{
	internal static class Program
	{
		private static int Main(string[] args)
		{
			if (args.Length != 1)
			{
				Console.Error.WriteLine("Usage: DiagnosticLogAnalyzer.exe <DiagnosticAnalysisInput.json>");
				return 2;
			}

			try
			{
				AnalysisInput input = AnalysisInputReader.Read(args[0]);
				string runDirectory = Path.GetDirectoryName(Path.GetFullPath(args[0]));
				DiagnosticLogParser parser = new DiagnosticLogParser();
				List<DiagnosticEvent> events = new List<DiagnosticEvent>();
				int issueCount = 0;

				foreach (DiagnosticLogFile log in input.DiagnosticLogs)
				{
					string path = AnalysisInputReader.ResolveSourcePath(runDirectory, log.Path);
					DiagnosticLogParseResult parsed = parser.Parse(path, log.Path);
					events.AddRange(parsed.Events);
					issueCount += parsed.Issues.Count;
					Console.WriteLine($"Diagnostic log: {log.Path}; events={parsed.Events.Count}; parse issues={parsed.Issues.Count}");

					foreach (DiagnosticLogParseIssue issue in parsed.Issues)
					{
						Console.WriteLine($"  Line {issue.LineNumber}: {issue.Message}");
					}
				}

				DiagnosticSessionCorrelationResult correlations = new DiagnosticSessionCorrelator().Correlate(input, events);
				Console.WriteLine($"RunId: {input.RunId}");
				Console.WriteLine($"Scenarios: {input.Scenarios.Count}; Performance sessions: {input.PerformanceSessions.Count}");
				Console.WriteLine($"Diagnostic events: {events.Count}; Parse issues: {issueCount}");
				Console.WriteLine($"Session correlation: window-matched={correlations.Events.Count(x => x.Status == DiagnosticSessionMatchStatus.WindowMatched)}; " +
					$"performance-only={correlations.Events.Count(x => x.Status == DiagnosticSessionMatchStatus.PerformanceOnly)}; " +
					$"unmatched={correlations.Events.Count(x => x.Status == DiagnosticSessionMatchStatus.Unmatched)}; " +
					$"ambiguous={correlations.Events.Count(x => x.Status == DiagnosticSessionMatchStatus.Ambiguous)}");
				int unmatchedWithCandidate = correlations.Events.Count(x =>
					x.Status == DiagnosticSessionMatchStatus.Unmatched && x.WindowCandidates.Count > 0);
				int multipleCandidates = correlations.Events.Count(x =>
					x.Status == DiagnosticSessionMatchStatus.Unmatched && x.WindowCandidates.Count > 1);
				Console.WriteLine($"Unmatched event window candidates: found={unmatchedWithCandidate}; multiple={multipleCandidates}; " +
					$"none={correlations.Events.Count(x => x.Status == DiagnosticSessionMatchStatus.Unmatched && x.WindowCandidates.Count == 0)}");
				foreach (var group in correlations.Events.Where(x => x.Status == DiagnosticSessionMatchStatus.Unmatched)
					.GroupBy(x => x.Event.SessionId, StringComparer.OrdinalIgnoreCase))
				{
					string description = string.Join(", ", group.SelectMany(x => x.WindowCandidates)
						.Select(x => $"{x.Scenario.Scenario} (window {x.Window.WindowOrder})").Distinct());
					Console.WriteLine($"  Unmatched SessionId={group.Key}: events={group.Count()}; " +
						$"candidate windows={(description.Length == 0 ? "none" : description)}");
				}
				foreach (string issue in correlations.Issues) { Console.WriteLine("  " + issue); }

				DiagnosticCommandCorrelationResult commandCorrelations = new DiagnosticCommandCorrelator().Correlate(correlations);
				Console.WriteLine($"Command correlation: matched={commandCorrelations.Events.Count(x => x.Status == DiagnosticCommandMatchStatus.Matched)}; " +
					$"no-confirmed-window={commandCorrelations.Events.Count(x => x.Status == DiagnosticCommandMatchStatus.NoConfirmedWindow)}; " +
					$"no-matching-command={commandCorrelations.Events.Count(x => x.Status == DiagnosticCommandMatchStatus.NoMatchingCommand)}; " +
					$"ambiguous={commandCorrelations.Events.Count(x => x.Status == DiagnosticCommandMatchStatus.Ambiguous)}; " +
					$"not-mapped-operation={commandCorrelations.Events.Count(x => x.Status == DiagnosticCommandMatchStatus.NotMappedOperation)}");

				DiagnosticCommandExecutionContextResult contexts = new DiagnosticCommandContextAnalyzer().Analyze(commandCorrelations);
				Console.WriteLine($"NoMatchingCommand execution context: observed-during-command={contexts.Events.Count(x => x.Status == DiagnosticCommandContextStatus.ObservedDuringCommand)}; " +
					$"outside-command={contexts.Events.Count(x => x.Status == DiagnosticCommandContextStatus.NoActiveCommand)}; " +
					$"multiple-active-commands={contexts.Events.Count(x => x.Status == DiagnosticCommandContextStatus.MultipleActiveCommands)}");
				foreach (var group in contexts.Events.Where(x => x.Status == DiagnosticCommandContextStatus.ObservedDuringCommand)
					.GroupBy(x => x.ActiveCommand.Type).OrderByDescending(x => x.Count()).ThenBy(x => x.Key))
				{
					Console.WriteLine($"  Observed during {group.Key}: {group.Count()} events");
				}
				DiagnosticEventLifecycleValidationResult lifecycles = new DiagnosticEventLifecycleValidator().Validate(commandCorrelations);
				Console.WriteLine($"Diagnostic lifecycle validation (Calculation, SaveData.Load, SaveData.Save, MainForm.Clear, Snapshot.Compare): pass={lifecycles.Lifecycles.Count(x => x.Verdict == DiagnosticLifecycleVerdict.Pass)}; " +
					$"fail={lifecycles.Lifecycles.Count(x => x.Verdict == DiagnosticLifecycleVerdict.Fail)}; " +
					$"inconclusive={lifecycles.Lifecycles.Count(x => x.Verdict == DiagnosticLifecycleVerdict.Inconclusive)}; " +
					$"excluded-events={lifecycles.ExcludedEvents}");
				foreach (DiagnosticEventLifecycle lifecycle in lifecycles.Lifecycles.Where(x => x.Verdict != DiagnosticLifecycleVerdict.Pass))
				{
					Console.WriteLine($"  {lifecycle.Verdict}: {lifecycle.Scenario.Scenario} (window {lifecycle.Window.WindowOrder}), " +
						$"line {lifecycle.Command.LineNumber}, {lifecycle.Operation}: {lifecycle.Reason}");
				}

				// 未対応・曖昧な相関は構文解析エラーではなく、後続の検証対象とする。
				return issueCount == 0 ? 0 : 1;
			}
			catch (Exception exception)
			{
				Console.Error.WriteLine(exception.Message);
				return 2;
			}
		}
	}
}
