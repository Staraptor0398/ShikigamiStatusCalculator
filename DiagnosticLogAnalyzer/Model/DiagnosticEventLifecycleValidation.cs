using System.Collections.Generic;
using DiagnosticLogAnalyzer.Model;

namespace DiagnosticLogAnalyzer.Model
{
	public enum DiagnosticLifecycleVerdict
	{
		Pass,
		Fail,
		Inconclusive
	}

	// PASSは診断イベントの整合性を意味し、Gui操作の成功を保証しない。
	public sealed class DiagnosticEventLifecycle
	{
		public ScenarioRecord Scenario { get; set; }
		public GuiWindowRecord Window { get; set; }
		public CommandRecord Command { get; set; }
		public string Operation { get; set; }
		public DiagnosticLifecycleVerdict Verdict { get; set; }
		public string Reason { get; set; }
		public List<DiagnosticCommandCorrelation> Events { get; } = new List<DiagnosticCommandCorrelation>();
	}

	public sealed class DiagnosticEventLifecycleValidationResult
	{
		public List<DiagnosticEventLifecycle> Lifecycles { get; } = new List<DiagnosticEventLifecycle>();
		public int ExcludedEvents { get; set; }
	}
}
