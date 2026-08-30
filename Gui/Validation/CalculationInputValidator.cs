using Gui.Common;
using Gui.Model;
using System.Collections.Generic;

namespace Gui.Validation
{
	public static class CalculationInputValidator
	{
		public static CalculationInputValidationOutcome Validate(MitamaSetInputModel inputModel)
		{
			CalculationInputValidationOutcome outcome;

			outcome = validateEquippedMitamaCount(inputModel.Mitamas);

			if (outcome != CalculationInputValidationOutcome.SUCCESS)
			{
				return outcome;
			}

			outcome = validateSubStatsWithoutMainStat(inputModel.Mitamas);

			if (outcome != CalculationInputValidationOutcome.SUCCESS)
			{
				return outcome;
			}

			outcome = validateEffectSlotCount(inputModel.Mitamas, inputModel.SetEffects, inputModel.UniqueEffects);

			if (outcome != CalculationInputValidationOutcome.SUCCESS)
			{
				return outcome;
			}

			outcome = validateSubStats(inputModel.Mitamas);

			if (outcome != CalculationInputValidationOutcome.SUCCESS)
			{
				return outcome;
			}

			return CalculationInputValidationOutcome.SUCCESS;
		}

		private static CalculationInputValidationOutcome validateEquippedMitamaCount(List<MitamaInputModel> mitamas)
		{
			if (getEquippedMitamaCount(mitamas) <= 0)
			{
				return CalculationInputValidationOutcome.NO_EQUIPPED_MITAMA;
			}

			return CalculationInputValidationOutcome.SUCCESS;
		}

		private static CalculationInputValidationOutcome validateSubStatsWithoutMainStat(List<MitamaInputModel> mitamas)
		{
			foreach (MitamaInputModel mitama in mitamas)
			{
				CalculationInputValidationOutcome outcome = validateSubStatsWithoutMainStat(mitama);

				if (outcome != CalculationInputValidationOutcome.SUCCESS)
				{
					return outcome;
				}
			}

			return CalculationInputValidationOutcome.SUCCESS;
		}

		private static CalculationInputValidationOutcome validateSubStatsWithoutMainStat(MitamaInputModel mitama)
		{
			if (!string.IsNullOrWhiteSpace(mitama.MainStat.Type))
			{
				return CalculationInputValidationOutcome.SUCCESS;
			}

			foreach (StatValueInputModel subStat in mitama.SubStat)
			{
				if (hasSubStatInput(subStat))
				{
					return CalculationInputValidationOutcome.MAIN_STAT_NOT_SELECTED_WITH_SUB_STAT;
				}
			}

			return CalculationInputValidationOutcome.SUCCESS;
		}

		private static bool hasSubStatInput(StatValueInputModel subStat)
		{
			return (!string.IsNullOrWhiteSpace(subStat.Type) && subStat.Type != DisplayText.NONE) || !string.IsNullOrWhiteSpace(subStat.ValueText);
		}

		private static CalculationInputValidationOutcome validateEffectSlotCount(List<MitamaInputModel> mitamas, List<SetEffectInputModel> setEffects, List<SetEffectInputModel> uniqueEffects)
		{
			int equippedMitamaCount = getEquippedMitamaCount(mitamas);
			int setEffectCount = getSelectedSetEffectCount(setEffects);
			int uniqueEffectCount = getSelectedUniqueEffectCount(uniqueEffects);

			int usedSlotCount = setEffectCount * 2 + uniqueEffectCount;

			if (usedSlotCount > equippedMitamaCount)
			{
				return CalculationInputValidationOutcome.EFFECT_SLOT_COUNT_EXCEEDS_EQUIPPED_SLOTS;
			}

			return CalculationInputValidationOutcome.SUCCESS;
		}

		private static int getEquippedMitamaCount(List<MitamaInputModel> mitamas)
		{
			int count = 0;

			foreach (MitamaInputModel mitama in mitamas)
			{
				if (!string.IsNullOrWhiteSpace(mitama.MainStat.Type))
				{
					count++;
				}
			}

			return count;
		}

		private static int getSelectedSetEffectCount(List<SetEffectInputModel> setEffects)
		{
			int count = 0;

			foreach (SetEffectInputModel setEffect in setEffects)
			{
				if (isSelectedEffect(setEffect))
				{
					count++;
				}
			}

			return count;
		}

		private static int getSelectedUniqueEffectCount(List<SetEffectInputModel> uniqueEffects)
		{
			int count = 0;

			foreach (SetEffectInputModel uniqueEffect in uniqueEffects)
			{
				if (isSelectedEffect(uniqueEffect))
				{
					count++;
				}
			}

			return count;
		}

		private static bool isSelectedEffect(SetEffectInputModel effect)
		{
			if (string.IsNullOrWhiteSpace(effect.Stat.Type))
			{
				return false;
			}

			if (effect.Stat.Type == DisplayText.NONE)
			{
				return false;
			}

			return true;
		}

		private static CalculationInputValidationOutcome validateSubStats(List<MitamaInputModel> mitamas)
		{
			foreach (MitamaInputModel mitama in mitamas)
			{
				// メインステータス未選択の場合は未装備御魂として扱う。
				// サブステータスが入力されている場合は、
				// validateSubStatsWithoutMainStat() で既にエラーとなる。
				if (string.IsNullOrWhiteSpace(mitama.MainStat.Type))
				{
					continue;
				}

				CalculationInputValidationOutcome outcome = validateSubStatsInMitama(mitama.SubStat);

				if (outcome != CalculationInputValidationOutcome.SUCCESS)
				{
					return outcome;
				}
			}

			return CalculationInputValidationOutcome.SUCCESS;
		}

		private static CalculationInputValidationOutcome validateSubStatsInMitama(List<StatValueInputModel> subStats)
		{
			List<string> selectedSubStats = new List<string>();

			foreach (StatValueInputModel subStat in subStats)
			{
				bool hasType = !string.IsNullOrWhiteSpace(subStat.Type) && subStat.Type != DisplayText.NONE;

				bool hasValue = !string.IsNullOrWhiteSpace(subStat.ValueText);

				if (!hasType && !hasValue)
				{
					continue;
				}

				if (hasType && !hasValue)
				{
					return CalculationInputValidationOutcome.SUB_STAT_TYPE_WITHOUT_VALUE;
				}

				if (!hasType && hasValue)
				{
					return CalculationInputValidationOutcome.SUB_STAT_VALUE_WITHOUT_TYPE;
				}

				if (!double.TryParse(subStat.ValueText, out double value))
				{
					return CalculationInputValidationOutcome.INVALID_VALUE;
				}

				if (value < 0)
				{
					return CalculationInputValidationOutcome.NEGATIVE_VALUE;
				}

				if (selectedSubStats.Contains(subStat.Type))
				{
					return CalculationInputValidationOutcome.DUPLICATE_SUB_STAT;
				}

				selectedSubStats.Add(subStat.Type);
			}

			if (selectedSubStats.Count < 3)
			{
				return CalculationInputValidationOutcome.SUB_STAT_COUNT_TOO_LOW;
			}

			return CalculationInputValidationOutcome.SUCCESS;
		}
	}
}
