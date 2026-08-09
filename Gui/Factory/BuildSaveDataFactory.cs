using Gui.Form.Control;
using Gui.SaveData;
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
				MitamaSet = MitamaSetSaveDataFactory.Create(slots, setEffectComboBoxes, uniqueEffectComboBoxes),
			};
		}
	}
}
