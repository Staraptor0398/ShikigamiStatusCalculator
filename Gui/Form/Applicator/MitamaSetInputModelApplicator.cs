using Gui.Form.Control;
using Gui.Logging;
using Gui.Model;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Gui.Form.Applicator
{
	public static class MitamaSetInputModelApplicator
	{
		public static void Apply(MitamaSetInputModel data, MitamaSlotInputControl[] slots, ComboBox[] setEffectComboBoxes, ComboBox[] uniqueEffectComboBoxes, PerformanceTracker tracker = null)
		{
			if (data == null)
			{
				return;
			}

			using (tracker?.Track("SaveData.Applicator.Apply"))
			{
				using (tracker?.Track("SaveData.Applicator.Mitama"))
				{
					applyMitama(data.Mitamas, slots, tracker);
				}

				using (tracker?.Track("SaveData.Applicator.SetEffect"))
				{
					applySetEffect(data.SetEffects, setEffectComboBoxes);
				}

				using (tracker?.Track("SaveData.Applicator.UniqueEffect"))
				{
					applyUniqueEffect(data.UniqueEffects, uniqueEffectComboBoxes);
				}
			}
		}

		private static void applyMitama(List<MitamaInputModel> list, MitamaSlotInputControl[] slots, PerformanceTracker tracker)
		{
			if (list == null)
			{
				return;
			}

			for (int i = 0; i < list.Count && i < slots.Length; i++)
			{
				applySingleMitama(list[i], slots[i], tracker);
			}
		}

		private static void applySingleMitama(MitamaInputModel data, MitamaSlotInputControl slot, PerformanceTracker tracker)
		{
			if (data == null || slot == null)
			{
				return;
			}

			using (tracker?.Track("SaveData.Applicator.MainStatType"))
			{
				if (string.IsNullOrEmpty(data.MainStat.Type))
				{
					slot.MainStatComboBox.SelectedIndex = -1;
					slot.MainStatComboBox.Text = "";
				}
				else
				{
					slot.MainStatComboBox.Text = data.MainStat.Type;
				}
			}

			using (tracker?.Track("SaveData.Applicator.MainStatValue"))
			{
				slot.MainValueTextBox.Text = data.MainStat.ValueText;
			}

			for (int i = 0; i < data.SubStat.Count && i < slot.SubStats.Length; i++)
			{
				applyStatValue(data.SubStat[i], slot.SubStats[i].TypeComboBox, slot.SubStats[i].ValueTextBox, tracker);
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

		private static void applyStatValue(StatValueInputModel data, ComboBox comboBox, TextBox textBox, PerformanceTracker tracker)
		{
			if (data == null || comboBox == null || textBox == null)
			{
				return;
			}

			using (tracker?.Track("SaveData.Applicator.SubStatType"))
			{
				applyStatValue(data, comboBox);
			}

			using (tracker?.Track("SaveData.Applicator.SubStatValue"))
			{
				textBox.Text = data.ValueText;
			}
		}

		private static void applyStatValue(StatValueInputModel data, ComboBox comboBox)
		{
			if (data == null || comboBox == null)
			{
				return;
			}

			if (string.IsNullOrEmpty(data.Type))
			{
				comboBox.SelectedIndex = -1;
			}
			else
			{
				comboBox.SelectedItem = data.Type;
			}
		}
	}
}
