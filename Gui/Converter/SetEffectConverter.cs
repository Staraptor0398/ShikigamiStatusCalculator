using SaveData.Model;

namespace Gui.Converter
{
	public static class SetEffectConverter
	{
		public static SetEffectSaveData ToSaveData(SetEffectDto dto)
		{
			if (dto == null)
			{
				return null;
			}

			return new SetEffectSaveData
			{
				Stat = StatValueConverter.ToSaveData(dto.Stat),
			};
		}
	}
}
