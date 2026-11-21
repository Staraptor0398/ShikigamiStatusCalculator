using System.Collections.Generic;

namespace DiagnosticLogAnalyzer.Model
{
	public enum DiagnosticCommandMatchStatus
	{
		Matched,
		NoConfirmedWindow,
		NoMatchingCommand,
		Ambiguous,
		NotMappedOperation
	}

	public sealed class DiagnosticCommandCorrelation
	{
		public DiagnosticEventCorrelation SessionCorrelation { get; set; }
		public DiagnosticCommandMatchStatus Status { get; set; }
		public CommandRecord Command { get; set; }
		public List<CommandRecord> Candidates { get; } = new List<CommandRecord>();
	}

	public sealed class DiagnosticCommandCorrelationResult
	{
		public List<DiagnosticCommandCorrelation> Events { get; } = new List<DiagnosticCommandCorrelation>();
	}
}