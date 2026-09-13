using System;
using System.Collections.Generic;

namespace ShikigamiDataAuditor.Model
{
	public class ExternalCacheEntry
	{
		public string Source { get; set; }
		public string Rarity { get; set; }
		public string ShikigamiName { get; set; }
		public string PageTitle { get; set; }
		public DateTime FetchedAt { get; set; }
		public long RevisionId { get; set; }
		public string SourceUrl { get; set; }
		public string RawContent { get; set; }
		public string ContentType { get; set; }
		public List<CachedStatus> Statuses { get; set; }

		public ExternalCacheEntry()
		{
			Statuses = new List<CachedStatus>();
		}
	}

	public class CachedStatus
	{
		public string Rarity { get; set; }
		public string Name { get; set; }
		public string StatScope { get; set; }
		public Dictionary<string, double> Values { get; set; }

		public CachedStatus()
		{
			Values = new Dictionary<string, double>();
		}
	}
}
