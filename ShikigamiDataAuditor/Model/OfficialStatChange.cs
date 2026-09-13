using System;

namespace ShikigamiDataAuditor.Model
{
	public class OfficialStatChange
	{
		public DateTime EffectiveDate { get; set; }
		public DateTime AnnouncementDate { get; set; }
		public string Rarity { get; set; }
		public string ShikigamiName { get; set; }
		public StatScope StatScope { get; set; }
		public StatType StatType { get; set; }
		public double? OldValue { get; set; }
		public double NewValue { get; set; }
		public string Unit { get; set; }
		public bool OldValueKnown { get; set; }
		public bool CurrentAppScope { get; set; }
		public string ReviewPriority { get; set; }
		public string AppAction { get; set; }
		public string SourceUrl { get; set; }
		public string Notes { get; set; }

		public string GetShikigamiKey()
		{
			return (Rarity ?? "").Trim().ToUpperInvariant() + "\t" + (ShikigamiName ?? "").Trim();
		}

		public string GetAuditKey()
		{
			return GetShikigamiKey() + "\t" + StatType;
		}
	}
}
