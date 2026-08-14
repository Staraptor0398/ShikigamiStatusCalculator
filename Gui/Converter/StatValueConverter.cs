using Gui.Model;
using SaveData.Model;

namespace Gui.Converter
{
	public static class StatValueConverter
	{
		public static StatValueSaveData ToSaveData(StatValueDto dto)
		{
			if (dto == null)
			{
				return null;
			}

			return new StatValueSaveData
			{
				Type = StatTypeConverter.ToText(dto.Type),
				Value = dto.Value
			};
		}

		public static StatValueDto ToDto(StatValueInputModel inputModel)
		{
			if (inputModel == null)
			{
				return null;
			}

			return new StatValueDto
			{
				Type = StatTypeConverter.ToDto(inputModel.Type),
				Value = inputModel.Value
			};
		}
	}
}
