using Gui.Common;
using Gui.Model;
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
				SubStats = dto.SubStat.Select(StatValueConverter.ToSaveData).ToList()
			};
		}

		public static MitamaSaveData ToSaveData(int slot, MitamaInputModel inputModel)
		{
			if (inputModel == null)
			{
				return null;
			}

			return new MitamaSaveData
			{
				Slot = slot,
				MainStat = StatValueConverter.ToSaveData(inputModel.MainStat),
				SubStats = inputModel.SubStat.Where(hasSubStatTypeAndValue).Select(StatValueConverter.ToSaveData).ToList()
			};
		}

		public static MitamaDto ToDto(MitamaInputModel inputModel)
		{
			if (inputModel == null)
			{
				return null;
			}

			return new MitamaDto
			{
				MainStat = StatValueConverter.ToDto(inputModel.MainStat),
				SubStat = inputModel.SubStat.Where(hasSubStatTypeAndValue).Select(StatValueConverter.ToDto).ToList()
			};
		}

		public static MitamaInputModel ToInputModel(MitamaSaveData saveData)
		{
			if (saveData == null)
			{
				return null;
			}

			return new MitamaInputModel
			{
				MainStat = StatValueConverter.ToInputModel(saveData.MainStat),
				SubStat = saveData.SubStats.Select(StatValueConverter.ToInputModel).ToList()
			};
		}

		private static bool hasSubStatTypeAndValue(StatValueInputModel subStat)
		{
			return !string.IsNullOrWhiteSpace(subStat.Type) && subStat.Type != DisplayText.NONE && !string.IsNullOrWhiteSpace(subStat.ValueText);
		}
	}
}
