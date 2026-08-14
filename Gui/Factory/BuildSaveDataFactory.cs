using Gui.Converter;
using Gui.Form.Control;
using SaveData.Model;
using System.Windows.Forms;

namespace Gui.Factory
{
	public static class BuildSaveDataFactory
	{
		public static BuildSaveData Create(ComboBox cmbShikigami, MitamaSlotInputControl[] slots, ComboBox[] setEffectComboBoxes, ComboBox[] uniqueEffectComboBoxes)
		{
			return new BuildSaveData
			{
				ShikigamiName = cmbShikigami.Text,
				MitamaSet = MitamaSetConverter.ToSaveData(MitamaSetInputModelFactory.Create(slots, setEffectComboBoxes, uniqueEffectComboBoxes))
			};
		}
	}
}
