using Gui.Form.Control;
using Gui.Model;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Gui.Form.Applicator
{
	public static class MitamaSetInputModelApplicator
	{
		public static void Apply(MitamaSetInputModel data, MitamaSlotInputControl[] slots, ComboBox[] setEffectComboBoxes, ComboBox[] uniqueEffectComboBoxes)
		{
			if (data == null)
			{
				return;
			}

			applyMitama(data.Mitamas, slots);
			applySetEffect(data.SetEffects, setEffectComboBoxes);
			applyUniqueEffect(data.UniqueEffects, uniqueEffectComboBoxes);
		}

		private static void applyMitama(List<MitamaInputModel> list, MitamaSlotInputControl[] slots)
		{
			if (list == null)
			{
				return;
			}

			for (int i = 0; i < list.Count && i < slots.Length; i++)
			{
				applySingleMitama(list[i], slots[i]);
			}
		}

		private static void applySingleMitama(MitamaInputModel data, MitamaSlotInputControl slot)
		{
			if (data == null || slot == null)
			{
				return;
			}

			slot.MainStatComboBox.Text = data.MainStat.Type;
			slot.MainValueTextBox.Text = data.MainStat.ValueText;

			for (int i = 0; i < data.SubStat.Count && i < slot.SubStats.Length; i++)
			{
				applyStatValue(data.SubStat[i], slot.SubStats[i].TypeComboBox, slot.SubStats[i].ValueTextBox);
			}
		}

		private static void applySetEffect(List<SetEffectInputModel> list, ComboBox[] setEffectComboBoxes)
		{
			if (list == null)
			{
				return;
			}

			for (int i = 0; i < list.Count && i < setEffectComboBoxes.Length; i++)
			{
				applySingleSetEffect(list[i], setEffectComboBoxes[i]);
			}
		}

		private static void applyUniqueEffect(List<SetEffectInputModel> list, ComboBox[] uniqueEffectComboBoxes)
		{
			if (list == null)
			{
				return;
			}

			for (int i = 0; i < list.Count && i < uniqueEffectComboBoxes.Length; i++)
			{
				applySingleSetEffect(list[i], uniqueEffectComboBoxes[i]);
			}
		}

		private static void applySingleSetEffect(SetEffectInputModel data, ComboBox comboBox)
		{
			if (data?.Stat == null || comboBox == null)
			{
				return;
			}

			applyStatValue(data.Stat, comboBox);
		}

		private static void applyStatValue(StatValueInputModel data, ComboBox comboBox, TextBox textBox)
		{
			if (data == null || comboBox == null || textBox == null)
			{
				return;
			}

			comboBox.SelectedItem = data.Type;
			textBox.Text = data.ValueText;
		}

		private static void applyStatValue(StatValueInputModel data, ComboBox comboBox)
		{
			if (data == null || comboBox == null)
			{
				return;
			}

			comboBox.SelectedItem = data.Type;
		}
	}
}
