using SaveData.Model;

namespace Gui.Converter
{
	public static class StatValueConverter
	{
		public static StatValueSaveData ToSaveData(StatValueDto dto)
		{
			return new StatValueSaveData
			{
				Type = StatTypeConverter.ToText(dto.Type),
				Value = dto.Value
			};
		}
	}
}
