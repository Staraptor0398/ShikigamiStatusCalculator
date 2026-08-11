using Gui.Converter;
using Gui.Form.Control;
using SaveData.Model;
using System;
using System.Windows.Forms;

namespace Gui.Factory
{
	public static class CalculationSnapshotSaveDataFactory
	{
		public static CalculationSnapshotSaveData Create(ComboBox cmbShikigami, MitamaSlotInputControl[] slots, ComboBox[] setEffectComboBoxes, ComboBox[] uniqueEffectComboBoxes, string snapshotName, CalculationResultDto calculationResult)
		{
			if (string.IsNullOrEmpty(snapshotName))
			{
				return null;
			}

			return new CalculationSnapshotSaveData
			{
				SnapshotName = snapshotName,
				CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
				ShikigamiName = cmbShikigami.Text,
				MitamaSet = MitamaSetSaveDataFactory.Create(slots, setEffectComboBoxes, uniqueEffectComboBoxes),
				MitamaStatus = StatusConverter.ToSaveData(calculationResult.MitamaOnlyStatus),
				FinalStatus = StatusConverter.ToSaveData(calculationResult.FinalStatus)
			};
		}
	}
}
