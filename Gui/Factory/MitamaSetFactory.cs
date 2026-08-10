using Gui.Converter;
using Gui.Form.Control;
using Gui.Resolver;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Gui.Factory
{
	public static class MitamaSetFactory
	{
		public static MitamaSetDto Create(MitamaSlotInputControl[] slots, ComboBox[] setEffectComboBoxes, ComboBox[] uniqueEffectComboBoxes)
		{
			MitamaSetDto dto = new MitamaSetDto();

			dto.Mitamas = new List<MitamaDto>();

			foreach (MitamaSlotInputControl slot in slots)
			{
				dto.Mitamas.Add(createMitamaDto(slot));
			}

			dto.SetEffects = createSetEffectDtos(setEffectComboBoxes);
			dto.UniqueEffects = createUniqueEffectDtos(uniqueEffectComboBoxes);

			return dto;
		}

		private static MitamaDto createMitamaDto(MitamaSlotInputControl slot)
		{
			if (slot == null)
			{
				return null;
			}

			return new MitamaDto
			{
				MainStat = createStatValueDto(slot.MainStatComboBox, slot.MainValueTextBox),
				SubStat = createSubStatValueDtos(slot.SubStats)
			};
		}

		private static List<StatValueDto> createSubStatValueDtos(SubStatInputControl[] subStats)
		{
			if (subStats == null)
			{
				return null;
			}

			List<StatValueDto> list = new List<StatValueDto>();

			foreach (SubStatInputControl subStat in subStats)
			{
				list.Add(createStatValueDto(subStat.TypeComboBox, subStat.ValueTextBox));
			}

			return list;
		}

		private static StatValueDto createStatValueDto(ComboBox cmbType, TextBox txtValue)
		{
			if (cmbType == null || txtValue == null)
			{
				return null;
			}

			var dto = new StatValueDto();

			dto.Type = StatTypeConverter.ToDto(cmbType.Text);

			if (double.TryParse(txtValue.Text, out double value))
			{
				dto.Value = value;
			}
			else
			{
				dto.Value = 0.0;
			}

			return dto;
		}

		private static List<SetEffectDto> createSetEffectDtos(ComboBox[] setEffectComboBoxes)
		{
			List<SetEffectDto> list = new List<SetEffectDto>();

			foreach (ComboBox comboBox in setEffectComboBoxes)
			{
				list.Add(createSetEffectDto(comboBox));
			}
			return list;
		}

		private static SetEffectDto createSetEffectDto(ComboBox cmbEffect)
		{
			if (cmbEffect == null)
			{
				return null;
			}

			return new SetEffectDto
			{
				Stat = new StatValueDto
				{
					Type = StatTypeConverter.ToDto(cmbEffect.Text),
					Value = MitamaEffectValueResolver.ResolveSetEffect(cmbEffect.Text)
				}
			};
		}

		private static List<SetEffectDto> createUniqueEffectDtos(ComboBox[] uniqueEffectComboBoxes)
		{
			List<SetEffectDto> list = new List<SetEffectDto>();

			foreach (ComboBox comboBox in uniqueEffectComboBoxes)
			{
				list.Add(createUniqueEffectDto(comboBox));
			}

			return list;
		}

		private static SetEffectDto createUniqueEffectDto(ComboBox cmbEffect)
		{
			if (cmbEffect == null)
			{
				return null;
			}

			return new SetEffectDto
			{
				Stat = new StatValueDto
				{
					Type = StatTypeConverter.ToDto(cmbEffect.Text),
					Value = MitamaEffectValueResolver.ResolveUniqueEffect(cmbEffect.Text)
				}
			};
		}
	}
}
