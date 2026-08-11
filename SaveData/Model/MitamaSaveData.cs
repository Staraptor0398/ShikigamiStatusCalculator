using System.Collections.Generic;

namespace SaveData.Model
{
	public class MitamaSaveData
	{
		public int Slot { get; set; }

		public StatValueSaveData MainStat { get; set; }

		public List<StatValueSaveData> SubStats { get; set; }
	}
}
