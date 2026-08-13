using Gui.Common;
using Gui.Form.Control;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Gui.Validation
{
	public static class CalculationInputValidator
	{
		public static CalculationInputValidationOutcome Validate(MitamaSlotInputControl[] slots, ComboBox[] setEffectComboBoxes, ComboBox[] uniqueEffectComboBoxes)
		{
			CalculationInputValidationOutcome outcome;

			outcome = validateEqueppedMitamaCount(slots);

			if (outcome != CalculationInputValidationOutcome.SUCCESS)
			{
				return outcome;
			}

			outcome = validateSubStatsInUnequippedSlots(slots);

			if (outcome != CalculationInputValidationOutcome.SUCCESS)
			{
				return outcome;
			}

			outcome = validateEffectSlotCount(slots, setEffectComboBoxes, uniqueEffectComboBoxes);

			if (outcome != CalculationInputValidationOutcome.SUCCESS)
			{
				return outcome;
			}

			outcome = validateSubStats(slots);

			if (outcome != CalculationInputValidationOutcome.SUCCESS)
			{
				return outcome;
			}

			return outcome;
		}

		private static CalculationInputValidationOutcome validateEqueppedMitamaCount(MitamaSlotInputControl[] slots)
		{
			if (getEquippedSlotCount(slots) <= 0)
			{
				return CalculationInputValidationOutcome.NO_EQUIPPED_MITAMA;
			}

			return CalculationInputValidationOutcome.SUCCESS;
		}

		private static CalculationInputValidationOutcome validateSubStatsInUnequippedSlots(MitamaSlotInputControl[] slots)
		{
			foreach (MitamaSlotInputControl slot in slots)
			{
				CalculationInputValidationOutcome outcome = validateSubStatsInUnequippedSlot(slot);

				if (outcome != CalculationInputValidationOutcome.SUCCESS)
				{
					return outcome;
				}
			}

			return CalculationInputValidationOutcome.SUCCESS;
		}

		private static CalculationInputValidationOutcome validateSubStatsInUnequippedSlot(MitamaSlotInputControl slot)
		{
			if (!string.IsNullOrWhiteSpace(slot.MainStatComboBox.Text))
			{
				return CalculationInputValidationOutcome.SUCCESS;
			}

			foreach (SubStatInputControl subsStat in slot.SubStats)
			{
				if (hasSubStatInput(subsStat.TypeComboBox, subsStat.ValueTextBox))
				{
					return CalculationInputValidationOutcome.MAIN_STAT_NOT_SELECTED_WITH_SUB_STAT;
				}
			}

			return CalculationInputValidationOutcome.SUCCESS;
		}

		private static bool hasSubStatInput(ComboBox cmbSubStat, TextBox txtSubvalue)
		{
			return (!string.IsNullOrWhiteSpace(cmbSubStat.Text) && cmbSubStat.Text != DisplayText.NONE) || !string.IsNullOrWhiteSpace(txtSubvalue.Text);
		}

		private static CalculationInputValidationOutcome validateEffectSlotCount(MitamaSlotInputControl[] slots, ComboBox[] setEffectComboBoxes, ComboBox[] uniqueEffectComboBoxes)
		{
			int equippedSlotCount = getEquippedSlotCount(slots);

			int setEffectCount = getSelectedSetEffectCount(setEffectComboBoxes);
			int uniqueEffectCount = getSelectedUniqueEffectCount(uniqueEffectComboBoxes);

			int usedSlotCount = setEffectCount * 2 + uniqueEffectCount;

			if (usedSlotCount > equippedSlotCount)
			{
				return CalculationInputValidationOutcome.EFFECT_SLOT_COUNT_EXCEEDS_EQUIPPED_SLOTS;
			}

			return CalculationInputValidationOutcome.SUCCESS;
		}

		private static int getEquippedSlotCount(MitamaSlotInputControl[] slots)
		{
			int count = 0;

			foreach (MitamaSlotInputControl slot in slots)
			{
				if (!string.IsNullOrWhiteSpace(slot.MainStatComboBox.Text))
				{
					count++;
				}
			}

			return count;
		}

		private static int getSelectedSetEffectCount(ComboBox[] setEffectComboBoxes)
		{
			int count = 0;

			foreach (ComboBox comboBox in setEffectComboBoxes)
			{
				if (isSelectedEffect(comboBox))
				{
					count++;
				}
			}

			return count;
		}

		private static int getSelectedUniqueEffectCount(ComboBox[] uniqueEffectComboBoxes)
		{
			int count = 0;

			foreach (ComboBox comboBox in uniqueEffectComboBoxes)
			{
				if (isSelectedEffect(comboBox))
				{
					count++;
				}
			}

			return count;
		}

		private static bool isSelectedEffect(ComboBox comboBox)
		{
			if (string.IsNullOrWhiteSpace(comboBox.Text))
			{
				return false;
			}

			if (comboBox.Text == DisplayText.NONE)
			{
				return false;
			}

			return true;
		}

		private static CalculationInputValidationOutcome validateSubStats(MitamaSlotInputControl[] slots)
		{
			foreach (MitamaSlotInputControl slot in slots)
			{
				CalculationInputValidationOutcome outcome = validateSubStatsInSlot(slot.SubStats);

				if (outcome != CalculationInputValidationOutcome.SUCCESS)
				{
					return outcome;
				}
			}

			return CalculationInputValidationOutcome.SUCCESS;
		}

		private static CalculationInputValidationOutcome validateSubStatsInSlot(SubStatInputControl[] subStats)
		{
			List<string> selectedSubStats = new List<string>();

			foreach (SubStatInputControl subStat in subStats)
			{
				bool hasType = !string.IsNullOrWhiteSpace(subStat.TypeComboBox.Text) && subStat.TypeComboBox.Text != DisplayText.NONE;
				bool hasValue = !string.IsNullOrWhiteSpace(subStat.ValueTextBox.Text);

				if (!hasType && !hasValue)
				{
					return CalculationInputValidationOutcome.SUCCESS;
				}

				if (hasType && !hasValue)
				{
					return CalculationInputValidationOutcome.SUB_STAT_TYPE_WHITHOUT_VALUE;
				}

				if (!hasType && hasValue)
				{
					return CalculationInputValidationOutcome.SUB_STAT_VALUE_WHITHOUT_TYPE;
				}

				if (!double.TryParse(subStat.ValueTextBox.Text, out double value))
				{
					return CalculationInputValidationOutcome.INVALID_VALUE;
				}

				if (value < 0)
				{
					return CalculationInputValidationOutcome.NEGATIVE_VALUE;
				}

				if (selectedSubStats.Contains(subStat.TypeComboBox.Text))
				{
					return CalculationInputValidationOutcome.DUPLICATE_SUB_STAT;
				}

				selectedSubStats.Add(subStat.TypeComboBox.Text);
			}

			return CalculationInputValidationOutcome.SUCCESS;
		}

	}
}
