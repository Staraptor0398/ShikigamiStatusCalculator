using DiagnosticLogAnalyzer.Correlation;
using DiagnosticLogAnalyzer.Model;
using DiagnosticLogAnalyzer.Parser;
using DiagnosticLogAnalyzer.Reader;
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

				foreach (string issue in correlations.Issues)
				{
					Console.WriteLine("  " + issue);
				}

				Console.WriteLine("Command correlation and rule validation are not implemented in this milestone.");

				// Missing or ambiguous correlations are not parse failures: they require later validation.
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
