using Gui.Model;
using SaveData.Model;
using System.Linq;

namespace Gui.Converter
{
	public static class MitamaSetConverter
	{
		public static MitamaSetSaveData ToSaveData(MitamaSetDto dto)
		{
			if (dto == null)
			{
				return null;
			}

			return new MitamaSetSaveData
			{
				Mitamas = dto.Mitamas.Select((mitama, index) => MitamaConverter.ToSaveData(index + 1, mitama)).ToList(),
				SetEffects = dto.SetEffects.Select(SetEffectConverter.ToSaveData).ToList(),
				UniqueEffects = dto.UniqueEffects.Select(SetEffectConverter.ToSaveData).ToList()
			};
		}

		public static MitamaSetDto ToDto(MitamaSetInputModel inputModel)
		{
			if (inputModel == null)
			{
				return null;
			}

			return new MitamaSetDto
			{
				Mitamas = inputModel.Mitamas.Select(MitamaConverter.ToDto).ToList(),
				SetEffects = inputModel.SetEffects.Select(SetEffectConverter.ToDto).ToList(),
				UniqueEffects = inputModel.UniqueEffects.Select(SetEffectConverter.ToDto).ToList()
			};
		}
	}
}
