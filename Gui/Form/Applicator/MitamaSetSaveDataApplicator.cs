using Gui.Form.Control;
using SaveData.Model;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Gui.Form.Applicator
{
	public static class MitamaSetSaveDataApplicator
	{
		public static void Apply(MitamaSetSaveData data, MitamaSlotInputControl[] slots, ComboBox[] setEffectComboBoxes, ComboBox[] uniqueEffectComboBoxes)
		{
			if (data == null)
			{
				return;
			}

			applyMitama(data.Mitamas, slots);
			applySetEffect(data.SetEffects, setEffectComboBoxes);
			applyUniqueEffect(data.UniqueEffects, uniqueEffectComboBoxes);
		}

		private static void applyMitama(List<MitamaSaveData> list, MitamaSlotInputControl[] slots)
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

		private static void applySingleMitama(MitamaSaveData data, MitamaSlotInputControl slot)
		{
			if (data == null || slot == null)
			{
				return;
			}

			slot.MainStatComboBox.Text = data.MainStat.Type;
			slot.MainValueTextBox.Text = data.MainStat.Value.ToString();

			for (int i = 0; i < data.SubStats.Count && i < slot.SubStats.Length; i++)
			{
				applyEffect(data.SubStats[i], slot.SubStats[i].TypeComboBox, slot.SubStats[i].ValueTextBox);
			}
		}

		private static void applySetEffect(List<EffectSaveData> list, ComboBox[] setEffectComboBoxes)
		{
			if (list == null)
			{
				return;
			}

			for (int i = 0; i < list.Count && i < setEffectComboBoxes.Length; i++)
			{
				applyEffect(list[i], setEffectComboBoxes[i]);
			}
		}

		private static void applyUniqueEffect(List<EffectSaveData> list, ComboBox[] uniqueEffectComboBoxes)
		{
			if (list == null)
			{
				return;
			}

			for (int i = 0; i < list.Count && i < uniqueEffectComboBoxes.Length; i++)
			{
				applyEffect(list[i], uniqueEffectComboBoxes[i]);
			}
		}

		private static void applyEffect(EffectSaveData data, ComboBox comboBox, TextBox textBox)
		{
			if (data == null || comboBox == null || textBox == null)
			{
				return;
			}

			comboBox.SelectedItem = data.Type;
			textBox.Text = data.Value.ToString();
		}

		private static void applyEffect(EffectSaveData data, ComboBox comboBox)
		{
			if (data == null || comboBox == null)
			{
				return;
			}

			comboBox.SelectedItem = data.Type;
		}
	}
}
