using System;

namespace ShikigamiDataAuditor.Model
{
	public class AuditResult
	{
		public AuditOutcome Outcome { get; set; }
		public string Rarity { get; set; }
		public string ShikigamiName { get; set; }
		public StatType StatType { get; set; }
		public double? AppValue { get; set; }
		public double? OfficialValue { get; set; }
		public double? ReferenceValue { get; set; }
		public StatScope StatScope { get; set; }
		public string Evidence { get; set; }
		public string SourceUrl { get; set; }
		public DateTime? FetchedAt { get; set; }
		public string Notes { get; set; }
	}
}
