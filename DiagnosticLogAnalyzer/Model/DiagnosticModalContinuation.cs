using System.Collections.Generic;

namespace DiagnosticLogAnalyzer.Model
{
	public enum DiagnosticModalContinuationStatus
	{
		Resolved,
		Unresolved,
		Ambiguous
	}

	// コマンド相関は変更せず、モーダル表示をまたぐ検証根拠を保持する。
	public sealed class DiagnosticModalContinuation
	{
		public DiagnosticEventLifecycle Lifecycle { get; set; }
		public DiagnosticModalContinuationStatus Status { get; set; }
		public DiagnosticCommandExecutionContext TerminalContext { get; set; }
		public string Reason { get; set; }
	}

	public sealed class DiagnosticModalContinuationResult
	{
		public List<DiagnosticModalContinuation> Items { get; } = new List<DiagnosticModalContinuation>();
	}
}