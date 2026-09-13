namespace ShikigamiDataAuditor.Model
{
	public class ExternalNameMapEntry
	{
		public string Rarity { get; set; }
		public string ShikigamiName { get; set; }
		public string Source { get; set; }
		public string PageTitle { get; set; }
		public bool Enabled { get; set; }
		public string Notes { get; set; }

		public string GetShikigamiKey()
		{
			return (Rarity ?? "").Trim().ToUpperInvariant() + "\t" + (ShikigamiName ?? "").Trim();
		}
	}
}
