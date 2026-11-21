using System.Collections.Generic;

namespace DiagnosticLogAnalyzer.Model
{
	public enum DiagnosticCommandContextStatus
	{
		ObservedDuringCommand,
		NoActiveCommand,
		MultipleActiveCommands
	}

	// コマンドとの因果関係を示さず、イベント発生時の実行状況だけを保持する。
	public sealed class DiagnosticCommandExecutionContext
	{
		public DiagnosticCommandCorrelation CommandCorrelation { get; set; }
		public DiagnosticCommandContextStatus Status { get; set; }
		public CommandRecord ActiveCommand { get; set; }
		public List<CommandRecord> ActiveCommands { get; } = new List<CommandRecord>();
	}

	public sealed class DiagnosticCommandExecutionContextResult
	{
		public List<DiagnosticCommandExecutionContext> Events { get; } = new List<DiagnosticCommandExecutionContext>();
	}
}
