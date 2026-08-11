using SaveData.Model;
using System.Linq;

namespace Gui.Converter
{
	public static class MitamaConverter
	{
		public static MitamaSaveData ToSaveData(int slot, MitamaDto dto)
		{
			if (dto == null)
			{
				return null;
			}

			return new MitamaSaveData
			{
				Slot = slot,
				MainStat = StatValueConverter.ToSaveData(dto.MainStat),
				SubStats = dto.SubStat.Select(StatValueConverter.ToSaveData).ToList(),
			};
		}
	}
}
