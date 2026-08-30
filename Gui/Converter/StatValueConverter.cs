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

		public static StatValueSaveData ToSaveData(StatValueInputModel inputModel)
		{
			if (inputModel == null)
			{
				return null;
			}

			if (string.IsNullOrWhiteSpace(inputModel.Type) && string.IsNullOrWhiteSpace(inputModel.ValueText))
			{
				return new StatValueSaveData
				{
					Type = inputModel.Type,
					Value = 0
				};
			}

			return new StatValueSaveData
			{
				Type = inputModel.Type,
				Value = inputModel.Value
			};
		}

		public static StatValueDto ToDto(StatValueInputModel inputModel)
		{
			if (inputModel == null)
			{
				return null;
			}

			if (string.IsNullOrWhiteSpace(inputModel.Type) && string.IsNullOrWhiteSpace(inputModel.ValueText))
			{
				return new StatValueDto
				{
					Type = StatTypeDto.None,
					Value = 0
				};
			}

			return new StatValueDto
			{
				Type = StatTypeConverter.ToDto(inputModel.Type),
				Value = inputModel.Value
			};
		}

		public static StatValueInputModel ToInputModel(StatValueSaveData saveData)
		{
			if (saveData == null)
			{
				return null;
			}

			return new StatValueInputModel
			{
				Type = saveData.Type,
				ValueText = saveData.Value.ToString()
			};
		}
	}
}
