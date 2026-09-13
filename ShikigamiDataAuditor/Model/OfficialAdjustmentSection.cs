using System.Collections.Generic;

namespace ShikigamiDataAuditor.Model
{
	public class OfficialAdjustmentSection
	{
		public string Rarity { get; set; }
		public string ShikigamiName { get; set; }
		public IReadOnlyList<string> Lines { get; set; }

		public OfficialAdjustmentSection()
		{
			Lines = new List<string>();
		}
	}
}
