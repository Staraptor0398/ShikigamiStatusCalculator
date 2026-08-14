using System.Collections.Generic;

namespace Gui.Model
{
	public class MitamaInputModel
	{
		public StatValueInputModel MainStat { get; set; }

		public List<StatValueInputModel> SubStat { get; set; } = new List<StatValueInputModel>();
	}
}
