using Gui.Form.Control;
using Gui.Resolver;
using Gui.SaveData;
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

				MainStat = new EffectSaveData
				{
					Type = input.MainStatComboBox.Text,
					Value = MainStatValueResolver.Resolve(input.MainStatComboBox.Text, slot)
				},

				SubStats = createSubStatSaveDataList(input.SubStats)
			};
		}

		private static List<EffectSaveData> createSubStatSaveDataList(SubStatInputControl[] subStats)
		{
			if (subStats == null)
			{
				return null;
			}

			List<EffectSaveData> list = new List<EffectSaveData>();

			foreach (SubStatInputControl subStat in subStats)
			{
				list.Add(createEffectSaveData(subStat.TypeComboBox, subStat.ValueTextBox));
			}

			return list;
		}

		private static List<EffectSaveData> createSetEffectSaveData(ComboBox[] setEffectComboBoxes)
		{
			List<EffectSaveData> list = new List<EffectSaveData>();

			foreach (ComboBox comboBox in setEffectComboBoxes)
			{
				list.Add(createEffectSaveData(comboBox));
			}

			return list;
		}

		private static List<EffectSaveData> createUniqueEffectSaveData(ComboBox[] uniqueEffectComboBoxes)
		{
			List<EffectSaveData> list = new List<EffectSaveData>();

			foreach (ComboBox comboBox in uniqueEffectComboBoxes)
			{
				list.Add(createEffectSaveData(comboBox));
			}

			return list;
		}

		private static EffectSaveData createEffectSaveData(ComboBox cmb, TextBox txt)
		{
			if (cmb == null || txt == null)
			{
				return null;
			}

			double.TryParse(txt.Text, out double value);

			return new EffectSaveData
			{
				Type = cmb.Text,
				Value = value
			};
		}

		private static EffectSaveData createEffectSaveData(ComboBox cmb)
		{
			if (cmb == null)
			{
				return null;
			}

			return new EffectSaveData
			{
				Type = cmb.Text,
				Value = 0.0
			};
		}
	}
}
