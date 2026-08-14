using Gui.Form.Control;
using Gui.Model;
using Gui.Resolver;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Gui.Factory
{
	public static class MitamaSetInputModelFactory
	{
		public static MitamaSetInputModel Create(MitamaSlotInputControl[] slots, ComboBox[] setEffectComboBoxes, ComboBox[] uniqueEffectComboBoxes)
		{
			return new MitamaSetInputModel
			{
				Mitamas = createMitamas(slots),
				SetEffects = createSetEffects(setEffectComboBoxes),
				UniqueEffects = createUniqueEffects(uniqueEffectComboBoxes)
			};
		}

		private static List<MitamaInputModel> createMitamas(MitamaSlotInputControl[] slots)
		{
			var list = new List<MitamaInputModel>();

			foreach (MitamaSlotInputControl slot in slots)
			{
				list.Add(createMitama(slot));
			}

			return list;
		}

		private static MitamaInputModel createMitama(MitamaSlotInputControl slot)
		{
			if (slot == null)
			{
				return null;
			}

			return new MitamaInputModel
			{
				MainStat = createStatValue(slot.MainStatComboBox, slot.MainValueTextBox),
				SubStat = createSubStats(slot.SubStats)
			};
		}

		private static List<StatValueInputModel> createSubStats(SubStatInputControl[] subStats)
		{
			if (subStats == null)
			{
				return null;
			}

			var list = new List<StatValueInputModel>();

			foreach (SubStatInputControl subStat in subStats)
			{
				list.Add(createStatValue(subStat.TypeComboBox, subStat.ValueTextBox));
			}

			return list;
		}

		private static StatValueInputModel createStatValue(ComboBox cmbType, TextBox txtValue)
		{
			if (cmbType == null || txtValue == null)
			{
				return null;
			}

			return new StatValueInputModel
			{
				Type = cmbType.Text,
				ValueText = txtValue.Text
			};
		}

		private static List<SetEffectInputModel> createSetEffects(ComboBox[] comboBoxes)
		{
			var list = new List<SetEffectInputModel>();

			foreach (ComboBox comboBox in comboBoxes)
			{
				list.Add(createSetEffect(comboBox));
			}

			return list;
		}

		private static SetEffectInputModel createSetEffect(ComboBox comboBox)
		{
			if (comboBox == null)
			{
				return null;
			}

			return new SetEffectInputModel
			{
				Stat = new StatValueInputModel
				{
					Type = comboBox.Text,
					ValueText = MitamaEffectValueResolver.ResolveSetEffect(comboBox.Text).ToString()
				}
			};
		}

		private static List<SetEffectInputModel> createUniqueEffects(ComboBox[] comboBoxes)
		{
			var list = new List<SetEffectInputModel>();

			foreach (ComboBox comboBox in comboBoxes)
			{
				list.Add(createUniqueEffect(comboBox));
			}

			return list;
		}

		private static SetEffectInputModel createUniqueEffect(ComboBox comboBox)
		{
			if (comboBox == null)
			{
				return null;
			}

			return new SetEffectInputModel
			{
				Stat = new StatValueInputModel
				{
					Type = comboBox.Text,
					ValueText = MitamaEffectValueResolver.ResolveUniqueEffect(comboBox.Text).ToString()
				}
			};
		}
	}
}
