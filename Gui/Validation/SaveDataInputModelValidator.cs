
using Gui.Form.Control;
using Gui.Model;
using System;
using System.Windows.Forms;

namespace Gui.Validation
{
	public static class SaveDataInputModelValidator
	{
		public static void Validate(MitamaSetInputModel inputModel, MitamaSlotInputControl[] slots, ComboBox[] setEffectComboBoxes, ComboBox[] uniqueEffectComboBoxes)
		{
			if (inputModel == null || inputModel.Mitamas == null || inputModel.SetEffects == null || inputModel.UniqueEffects == null)
			{
				throw new InvalidOperationException("SaveData input model is incomplete.");
			}

			if (slots == null || setEffectComboBoxes == null || uniqueEffectComboBoxes == null)
			{
				throw new InvalidOperationException("SaveData UI controls are not initialized.");
			}

			if (inputModel.Mitamas.Count > slots.Length || inputModel.SetEffects.Count > setEffectComboBoxes.Length || inputModel.UniqueEffects.Count > uniqueEffectComboBoxes.Length)
			{
				throw new InvalidOperationException("SaveData exceeds the available UI slots.");
			}

			for (int i = 0; i < inputModel.Mitamas.Count; i++)
			{
				validateMitama(inputModel.Mitamas[i], slots[i], i);
			}

			validateEffects(inputModel.SetEffects, setEffectComboBoxes, "SetEffect");
			validateEffects(inputModel.UniqueEffects, uniqueEffectComboBoxes, "UniqueEffect");
		}

		private static void validateMitama(MitamaInputModel mitama, MitamaSlotInputControl slot, int index)
		{
			if (mitama == null || mitama.MainStat == null || mitama.SubStat == null || slot == null || slot.MainStatComboBox == null || slot.MainValueTextBox == null || slot.SubStats == null)
			{
				throw new InvalidOperationException($"Invalid Mitama structure at slot {index + 1}.");
			}

			if (mitama.SubStat.Count > slot.SubStats.Length)
			{
				throw new InvalidOperationException($"Too many sub stats at slot {index + 1}.");
			}

			validateStatValue(mitama.MainStat, slot.MainStatComboBox, $"Mitama[{index}].MainStat");

			for (int i = 0; i < mitama.SubStat.Count; i++)
			{
				SubStatInputControl subStat = slot.SubStats[i];

				if (subStat == null || subStat.TypeComboBox == null || subStat.ValueTextBox == null)
				{
					throw new InvalidOperationException($"Invalid sub stat UI control at slot {index + 1}.");
				}

				validateStatValue(mitama.SubStat[i], subStat.TypeComboBox, $"Mitama[{index}].SubStat[{i}]");
			}
		}

		private static void validateEffects(System.Collections.Generic.List<SetEffectInputModel> effects, ComboBox[] comboBoxes, string effectName)
		{
			for (int i = 0; i < effects.Count; i++)
			{
				if (effects[i] == null || effects[i].Stat == null)
				{
					throw new InvalidOperationException($"Invalid {effectName} at index {i}.");
				}

				validateStatValue(effects[i].Stat, comboBoxes[i], $"{effectName}[{i}]");
			}
		}

		private static void validateStatValue(StatValueInputModel stat, ComboBox comboBox, string path)
		{
			if (stat == null || comboBox == null || stat.Type == null || stat.ValueText == null)
			{
				throw new InvalidOperationException($"Invalid stat value: {path}");
			}

			if (stat.Type.Length == 0)
			{
				return;
			}

			if (!comboBox.Items.Contains(stat.Type))
			{
				throw new InvalidOperationException($"Unsupported stat type: {path}, Type={stat.Type}");
			}
		}
	}
}
