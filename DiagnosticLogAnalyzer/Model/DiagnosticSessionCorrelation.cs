using System.Collections.Generic;

namespace DiagnosticLogAnalyzer.Model
{
	public enum DiagnosticSessionMatchStatus
	{
		WindowMatched,
		PerformanceOnly,
		Unmatched,
		Ambiguous
	}

	public sealed class DiagnosticEventCorrelation
	{
		public DiagnosticEvent Event { get; set; }
		public DiagnosticSessionMatchStatus Status { get; set; }
		public ScenarioRecord Scenario { get; set; }
		public GuiWindowRecord Window { get; set; }
		public PerformanceSessionRecord PerformanceSession { get; set; }
		public string Reason { get; set; }
		public List<DiagnosticWindowCandidate> WindowCandidates { get; } = new List<DiagnosticWindowCandidate>();
	}

	// SessionIdで確定できなかったイベントに対する時刻ベースの参考候補。
	public sealed class DiagnosticWindowCandidate
	{
		public ScenarioRecord Scenario { get; set; }
		public GuiWindowRecord Window { get; set; }
	}

	public sealed class DiagnosticSessionCorrelationResult
	{
		public List<DiagnosticEventCorrelation> Events { get; } = new List<DiagnosticEventCorrelation>();
		public List<string> Issues { get; } = new List<string>();
	}
}
