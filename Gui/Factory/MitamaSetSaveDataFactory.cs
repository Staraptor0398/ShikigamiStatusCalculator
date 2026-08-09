using Gui.Common;
using Gui.Form.Control;
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
					Value = getMainStatValue(input.MainValueTextBox.Text, slot)
				},

				SubStats = createSubStatSaveDataList(input.SubStats)
			};
		}

		private static double getMainStatValue(string text, int slot)
		{
			double ret = 0.0;

			switch (slot)
			{
				case 1:
					ret = 486.0;
					break;
				case 2:
					if (text == DisplayText.Speed)
					{
						ret = 57.0;
					}
					else
					{
						ret = 55.0;
					}
					break;
				case 3:
					ret = 104.0;
					break;
				case 4:
					ret = 55.0;
					break;
				case 5:
					ret = 2052.0;
					break;
				case 6:
					if (text == DisplayText.CriticalDamage)
					{
						ret = 89.0;
					}
					else
					{
						ret = 55.0;
					}
					break;
				default:
					break;
			}

			return ret;
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
