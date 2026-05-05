using System.Collections.Generic;

namespace Gui.SaveData
{
	public class MitamaSetSaveData
	{
		public List<MitamaSaveData> Mitamas { get; set; }

		public List<EffectSaveData> SetEffects { get; set; }

		public List<EffectSaveData> UniqueEffects { get; set; }
	}
}