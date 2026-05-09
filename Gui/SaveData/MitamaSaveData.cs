using System.Collections.Generic;

namespace Gui.SaveData
{
	public class MitamaSaveData
	{
		public int Slot { get; set; }

		public EffectSaveData MainStat { get; set; }

		public List<EffectSaveData> SubStats { get; set; }
	}
}