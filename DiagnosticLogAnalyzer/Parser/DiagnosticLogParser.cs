using DiagnosticLogAnalyzer.Model;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace DiagnosticLogAnalyzer.Parser
{
	public sealed class DiagnosticLogParser
	{
		private static readonly Regex EventPattern = new Regex(@"^(?<timestamp>\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3}) \[(?<level>INFO|WARNING|ERROR)\] SessionId=(?<session>[0-9a-fA-F]{32}) Operation=(?<operation>\S+) Event=(?<event>\S+)(?: (?<details>.*))?$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

		public DiagnosticLogParseResult Parse(string fullPath, string sourceFile)
		{
			DiagnosticLogParseResult result = new DiagnosticLogParseResult();
			int lineNumber = 0;

			using (StreamReader reader = new StreamReader(fullPath, Encoding.UTF8, true))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					lineNumber++;
					if (string.IsNullOrWhiteSpace(line))
					{
						continue;
					}
					Match match = EventPattern.Match(line);
					DateTime timestamp;

					if (!match.Success || !DateTime.TryParseExact(match.Groups["timestamp"].Value, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out timestamp))
					{
						result.Issues.Add(new DiagnosticLogParseIssue
						{
							SourceFile = sourceFile,
							LineNumber = lineNumber,
							Message = "Invalid diagnostic event record."
						});
						continue;
					}

					result.Events.Add(new DiagnosticEvent
					{
						SourceFile = sourceFile,
						LineNumber = lineNumber,
						Timestamp = timestamp,
						Level = match.Groups["level"].Value,
						SessionId = match.Groups["session"].Value.ToLowerInvariant(),
						Operation = match.Groups["operation"].Value,
						EventName = match.Groups["event"].Value,
						Details = match.Groups["details"].Success ? match.Groups["details"].Value : ""
					});
				}
			}

			return result;
		}
	}
}
