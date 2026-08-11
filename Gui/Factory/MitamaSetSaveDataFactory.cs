using Gui.Form.Control;
using Gui.Resolver;
using SaveData.Model;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Gui.Factory
{
	public static class MitamaSetSaveDataFactory
	{
		public static MitamaSetSaveData Create(MitamaSlotInputControl[] slots, ComboBox[] setEffectComboBoxes, ComboBox[] uniqueEffectComboBoxes)
		{
			return new MitamaSetSaveData
			{
				Mitamas = createMitamaSaveDataList(slots),
				SetEffects = createSetEffectSaveData(setEffectComboBoxes),
				UniqueEffects = createUniqueEffectSaveData(uniqueEffectComboBoxes)
			};
		}

		private static List<MitamaSaveData> createMitamaSaveDataList(MitamaSlotInputControl[] slots)
		{
			List<MitamaSaveData> list = new List<MitamaSaveData>();

			for (int i = 0; i < slots.Length; i++)
			{
				list.Add(createMitamaSaveData(i + 1, slots[i]));
			}

			return list;
		}

		private static MitamaSaveData createMitamaSaveData(int slot, MitamaSlotInputControl input)
		{
			if (0 > slot || slot > 6 || input == null)
			{
				return null;
			}

			return new MitamaSaveData
			{
				Slot = slot,

				MainStat = new StatValueSaveData
				{
					Type = input.MainStatComboBox.Text,
					Value = MainStatValueResolver.Resolve(input.MainStatComboBox.Text, slot)
				},

				SubStats = createSubStatSaveDataList(input.SubStats)
			};
		}

		private static List<StatValueSaveData> createSubStatSaveDataList(SubStatInputControl[] subStats)
		{
			if (subStats == null)
			{
				return null;
			}

			List<StatValueSaveData> list = new List<StatValueSaveData>();

			foreach (SubStatInputControl subStat in subStats)
			{
				list.Add(createStatValueSaveData(subStat.TypeComboBox, subStat.ValueTextBox));
			}

			return list;
		}

		private static List<SetEffectSaveData> createSetEffectSaveData(ComboBox[] setEffectComboBoxes)
		{
			List<SetEffectSaveData> list = new List<SetEffectSaveData>();

			foreach (ComboBox comboBox in setEffectComboBoxes)
			{
				list.Add(createSetEffectSaveData(comboBox));
			}

			return list;
		}

		private static List<SetEffectSaveData> createUniqueEffectSaveData(ComboBox[] uniqueEffectComboBoxes)
		{
			List<SetEffectSaveData> list = new List<SetEffectSaveData>();

			foreach (ComboBox comboBox in uniqueEffectComboBoxes)
			{
				list.Add(createSetEffectSaveData(comboBox));
			}

			return list;
		}

		private static StatValueSaveData createStatValueSaveData(ComboBox cmb, TextBox txt)
		{
			if (cmb == null || txt == null)
			{
				return null;
			}

			double.TryParse(txt.Text, out double value);

			return new StatValueSaveData
			{
				Type = cmb.Text,
				Value = value
			};
		}

		private static SetEffectSaveData createSetEffectSaveData(ComboBox cmb)
		{
			if (cmb == null)
			{
				return null;
			}

			return new SetEffectSaveData
			{
				Stat = new StatValueSaveData
				{
					Type = cmb.Text,
					Value = 0.0
				}
			};
		}
	}
}
