using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DiagnosticLogAnalyzer.Model
{
	[DataContract]
	public sealed class AnalysisInput
	{
		[DataMember(Name = "schemaVersion", IsRequired = true)]
		public int SchemaVersion { get; set; }

		[DataMember(Name = "runId", IsRequired = true)]
		public string RunId { get; set; }

		[DataMember(Name = "diagnosticLogs", IsRequired = true)]
		public List<DiagnosticLogFile> DiagnosticLogs { get; set; }

		[DataMember(Name = "scenarios", IsRequired = true)]
		public List<ScenarioRecord> Scenarios { get; set; }

		[DataMember(Name = "performanceSessions", IsRequired = true)]
		public List<PerformanceSessionRecord> PerformanceSessions { get; set; }

		[DataMember(Name = "preparationIssues", IsRequired = true)]
		public List<string> PreparationIssues { get; set; }
	}

	[DataContract]
	public sealed class DiagnosticLogFile
	{
		[DataMember(Name = "path", IsRequired = true)]
		public string Path { get; set; }
	}

	[DataContract]
	public sealed class ScenarioRecord
	{
		[DataMember(Name = "scenario", IsRequired = true)]
		public string Scenario { get; set; }

		[DataMember(Name = "sourceFile", IsRequired = true)]
		public string SourceFile { get; set; }

		[DataMember(Name = "result", IsRequired = true)]
		public string Result { get; set; }

		[DataMember(Name = "start", IsRequired = true)]
		public string Start { get; set; }

		[DataMember(Name = "end", IsRequired = true)]
		public string End { get; set; }

		[DataMember(Name = "commands", IsRequired = true)]
		public List<CommandRecord> Commands { get; set; }

		[DataMember(Name = "guiWindows", IsRequired = true)]
		public List<GuiWindowRecord> GuiWindows { get; set; }
	}

	[DataContract]
	public sealed class CommandRecord
	{
		[DataMember(Name = "lineNumber", IsRequired = true)]
		public int LineNumber { get; set; }

		[DataMember(Name = "type", IsRequired = true)]
		public string Type { get; set; }

		[DataMember(Name = "text", IsRequired = true)]
		public string Text { get; set; }

		[DataMember(Name = "result", IsRequired = true)]
		public string Result { get; set; }

		[DataMember(Name = "start", IsRequired = true)]
		public string Start { get; set; }

		[DataMember(Name = "end", IsRequired = true)]
		public string End { get; set; }

		[DataMember(Name = "preciseTime", IsRequired = true)]
		public bool PreciseTime { get; set; }
	}

	[DataContract]
	public sealed class GuiWindowRecord
	{
		[DataMember(Name = "windowOrder", IsRequired = true)]
		public int WindowOrder { get; set; }

		[DataMember(Name = "openCommand", IsRequired = true)]
		public string OpenCommand { get; set; }

		[DataMember(Name = "openLine", IsRequired = true)]
		public int OpenLine { get; set; }

		[DataMember(Name = "closeLine", IsRequired = true)]
		public int CloseLine { get; set; }

		[DataMember(Name = "start", IsRequired = true)]
		public string Start { get; set; }

		[DataMember(Name = "end", IsRequired = true)]
		public string End { get; set; }

		[DataMember(Name = "openPassed", IsRequired = true)]
		public bool OpenPassed { get; set; }

		[DataMember(Name = "preciseTime", IsRequired = true)]
		public bool PreciseTime { get; set; }

		[DataMember(Name = "sessionId", IsRequired = true)]
		public string SessionId { get; set; }

		[DataMember(Name = "matchQuality", IsRequired = true)]
		public string MatchQuality { get; set; }
	}

	[DataContract]
	public sealed class PerformanceSessionRecord
	{
		[DataMember(Name = "sessionId", IsRequired = true)]
		public string SessionId { get; set; }

		[DataMember(Name = "sessionOrder", IsRequired = true)]
		public int SessionOrder { get; set; }

		[DataMember(Name = "endTime", IsRequired = true)]
		public string EndTime { get; set; }

		[DataMember(Name = "matchQuality", IsRequired = true)]
		public string MatchQuality { get; set; }

		[DataMember(Name = "scenarioFile", IsRequired = true)]
		public string ScenarioFile { get; set; }

		[DataMember(Name = "windowOrder", IsRequired = true)]
		public int? WindowOrder { get; set; }
	}
}
