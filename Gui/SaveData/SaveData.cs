using System.Collections.Generic;

namespace Gui.SaveData
{
	public class SaveData
	{
		public string ShikigamiName { get; set; }

		public List<MitamaSaveData> Mitamas { get; set; }

		public List<EffectSaveData> SetEffects { get; set; }

		public List<EffectSaveData> UniqueEffects {  get; set; }
	}
}