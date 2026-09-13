using System;
using System.Collections.Generic;

namespace ShikigamiDataAuditor.Model
{
	public class OfficialAnnouncement
	{
		public string SourceUrl { get; set; }
		public DateTime AnnouncementDate { get; set; }
		public DateTime EffectiveDate { get; set; }
		public IReadOnlyList<string> Lines { get; set; }

		public OfficialAnnouncement()
		{
			Lines = new List<string>();
		}
	}
}
