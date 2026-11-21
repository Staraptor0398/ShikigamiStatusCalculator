using DiagnosticLogAnalyzer.Model;
using DiagnosticLogAnalyzer.Parser;
using DiagnosticLogAnalyzer.Reader;
using System;
using System.IO;

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
				int eventCount = 0;
				int issueCount = 0;

				foreach (DiagnosticLogFile log in input.DiagnosticLogs)
				{
					string path = AnalysisInputReader.ResolveSourcePath(runDirectory, log.Path);
					DiagnosticLogParseResult result = parser.Parse(path, log.Path);
					eventCount += result.Events.Count;
					issueCount += result.Issues.Count;
					Console.WriteLine($"Diagnostic log: {log.Path}; events={result.Events.Count}; parse issues={result.Issues.Count}");

					foreach (DiagnosticLogParseIssue issue in result.Issues)
					{
						Console.WriteLine($"  Line {issue.LineNumber}: {issue.Message}");
					}
				}

				Console.WriteLine($"RunId: {input.RunId}");
				Console.WriteLine($"Scenarios: {input.Scenarios.Count}; Performance sessions: {input.PerformanceSessions.Count}");
				Console.WriteLine($"Diagnostic events: {eventCount}; Parse issues: {issueCount}");
				Console.WriteLine("Session correlation and rule validation are not implemented in this milestone.");

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
