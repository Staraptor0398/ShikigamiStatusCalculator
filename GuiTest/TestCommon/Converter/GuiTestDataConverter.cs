using Gui.Converter;
using Gui.Model;
using GuiTest.TestCommon.Model;
using SaveData.Model;

namespace GuiTest.TestCommon.Converter
{
	public static class GuiTestDataConverter
	{
		// StatValueInputModel -> StatValueTestData
		public static StatValueTestData ToTestData(StatValueInputModel inputModel)
		{
			if (inputModel == null)
			{
				return null;
			}

			return new StatValueTestData
			{
				Type = normalizeStatType(inputModel.Type),
				Value = inputModel.Value
			};
		}

		// StatValueDto -> StatValueTestData
		public static StatValueTestData ToTestData(StatValueDto dto)
		{
			if (dto == null)
			{
				return null;
			}

			return new StatValueTestData
			{
				Type = dto.Type.ToString(),
				Value = dto.Value
			};
		}

		// StatValueSaveData -> StatValueTestData
		public static StatValueTestData ToTestData(StatValueSaveData saveData)
		{
			if (saveData == null)
			{
				return null;
			}

			return new StatValueTestData
			{
				Type = normalizeStatType(saveData.Type),
				Value = saveData.Value
			};
		}

		// MitamaInputModel -> MitamaTestData
		public static MitamaTestData ToTestData(MitamaInputModel inputModel)
		{
			if (inputModel == null)
			{
				return null;
			}

			return new MitamaTestData
			{
				MainStat = ToTestData(inputModel.MainStat),
				SubStat = inputModel.SubStat.Select(stat => ToTestData(stat)).ToList()
			};
		}

		// MitamaDto -> MitamaTestData
		public static MitamaTestData ToTestData(MitamaDto dto)
		{
			if (dto == null)
			{
				return null;
			}

			return new MitamaTestData
			{
				MainStat = ToTestData(dto.MainStat),
				SubStat = dto.SubStat.Select(stat => ToTestData(stat)).ToList()
			};
		}

		// MitamaSaveData -> MitamaTestData
		public static MitamaTestData ToTestData(MitamaSaveData saveData)
		{
			if (saveData == null)
			{
				return null;
			}

			return new MitamaTestData
			{
				MainStat = ToTestData(saveData.MainStat),
				SubStat = saveData.SubStats.Select(stat => ToTestData(stat)).ToList()
			};
		}

		// SetEffectInputModel -> SetEffectTestData
		public static SetEffectTestData ToTestData(SetEffectInputModel inputModel)
		{
			if (inputModel == null)
			{
				return null;
			}

			return new SetEffectTestData
			{
				Stat = ToTestData(inputModel.Stat)
			};
		}

		// SetEffectDto -> SetEffectTestData
		public static SetEffectTestData ToTestData(SetEffectDto dto)
		{
			if (dto == null)
			{
				return null;
			}

			return new SetEffectTestData
			{
				Stat = ToTestData(dto.Stat)
			};
		}

		// SetEffectSaveData -> SetEffectTestData
		public static SetEffectTestData ToTestData(SetEffectSaveData saveData)
		{
			if (saveData == null)
			{
				return null;
			}

			return new SetEffectTestData
			{
				Stat = ToTestData(saveData.Stat)
			};
		}

		// MitamaSetInputModel -> MitamaSetTestData
		public static MitamaSetTestData ToTestData(MitamaSetInputModel inputModel)
		{
			if (inputModel == null)
			{
				return null;
			}

			return new MitamaSetTestData
			{
				Mitamas = inputModel.Mitamas.Select(mitama => ToTestData(mitama)).ToList(),
				SetEffects = inputModel.SetEffects.Select(effect => ToTestData(effect)).ToList(),
				UniqueEffects = inputModel.UniqueEffects.Select(effect => ToTestData(effect)).ToList()
			};
		}

		// MitamaSetDto -> MitamaSetTestData
		public static MitamaSetTestData ToTestData(MitamaSetDto dto)
		{
			if (dto == null)
			{
				return null;
			}

			return new MitamaSetTestData
			{
				Mitamas = dto.Mitamas.Select(mitama => ToTestData(mitama)).ToList(),
				SetEffects = dto.SetEffects.Select(effect => ToTestData(effect)).ToList(),
				UniqueEffects = dto.UniqueEffects.Select(effect => ToTestData(effect)).ToList()
			};
		}

		// MitamaSetSaveData -> MitamaSetTestData
		public static MitamaSetTestData ToTestData(MitamaSetSaveData saveData)
		{
			if (saveData == null)
			{
				return null;
			}

			return new MitamaSetTestData
			{
				Mitamas = saveData.Mitamas.Select(mitama => ToTestData(mitama)).ToList(),
				SetEffects = saveData.SetEffects.Select(effect => ToTestData(effect)).ToList(),
				UniqueEffects = saveData.UniqueEffects.Select(effect => ToTestData(effect)).ToList()
			};
		}

		private static string normalizeStatType(string type)
		{
			if (Enum.TryParse(type, out StatTypeDto dto))
			{
				return dto.ToString();
			}

			return StatTypeConverter.ToDto(type).ToString();
		}
	}
}
