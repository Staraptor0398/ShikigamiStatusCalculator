using ShikigamiDataAuditor.Model;
using System.Collections.Generic;

namespace ShikigamiDataAuditor.DataGeneration
{
	public class OfficialChangeHistoryValidationResult
	{
		public int SeedCount { get; set; }
		public int MatchedCount { get; set; }
		public IReadOnlyList<OfficialStatChange> ExtraChanges { get; set; }

		public int ExtraCount
		{
			get
			{
				return ExtraChanges == null ? 0 : ExtraChanges.Count;
			}
		}

		public OfficialChangeHistoryValidationResult()
		{
			ExtraChanges = new List<OfficialStatChange>();
		}
	}
}
