using System.Collections.Generic;

namespace Gui.Model
{
	public class MitamaSetInputModel
	{
		public List<MitamaInputModel> Mitamas { get; set; } = new List<MitamaInputModel>();

		public List<SetEffectInputModel> SetEffects { get; set; } = new List<SetEffectInputModel>();

		public List<SetEffectInputModel> UniqueEffects { get; set; } = new List<SetEffectInputModel>();
	}
}
