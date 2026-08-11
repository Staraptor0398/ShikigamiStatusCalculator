using System.Collections.Generic;

namespace SaveData.Model
{
	public class MitamaSetSaveData
	{
		public List<MitamaSaveData> Mitamas { get; set; }

		public List<SetEffectSaveData> SetEffects { get; set; }

		public List<SetEffectSaveData> UniqueEffects { get; set; }
	}
}
