using System;
using System.Collections.Generic;

namespace ShikigamiDataAuditor.Model
{
	public class ExternalStatusResult
	{
		public IReadOnlyList<ShikigamiStatus> Statuses { get; set; }
		public DateTime FetchedAt { get; set; }
		public string SourceUrl { get; set; }
		public long RevisionId { get; set; }
		public bool IsCache { get; set; }
		public bool IsStale { get; set; }
		public bool Succeeded { get; set; }
		public string ErrorMessage { get; set; }

		public ExternalStatusResult()
		{
			Statuses = new List<ShikigamiStatus>();
		}
	}
}
