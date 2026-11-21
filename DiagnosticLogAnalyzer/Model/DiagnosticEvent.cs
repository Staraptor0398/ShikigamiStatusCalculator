using System;
using System.Collections.Generic;

namespace DiagnosticLogAnalyzer.Model
{
	public sealed class DiagnosticEvent
	{
		public string SourceFile { get; set; }
		public int LineNumber { get; set; }
		public DateTime Timestamp { get; set; }
		public string Level { get; set; }
		public string SessionId { get; set; }
		public string Operation { get; set; }
		public string EventName { get; set; }
		public string Details { get; set; }
	}

	public sealed class DiagnosticLogParseIssue
	{
		public string SourceFile { get; set; }
		public int LineNumber { get; set; }
		public string Message { get; set; }
	}

	public sealed class DiagnosticLogParseResult
	{
		public List<DiagnosticEvent> Events { get; } = new List<DiagnosticEvent>();
		public List<DiagnosticLogParseIssue> Issues { get; } = new List<DiagnosticLogParseIssue>();
	}
}
