namespace Gui.SaveData
{
	public class CalculationSnapshotSaveData
	{
		public string SnapshotName { get; set; }

		public string CreatedAt { get; set; }

		public string ShikigamiName { get; set; }

		public MitamaSetSaveData MitamaSet { get; set; }

		public StatusSaveData MitamaStatus { get; set; }

		public StatusSaveData FinalStatus { get; set; }
	}
}
