using SaveData.Access;
using SaveData.Model;

namespace GuiTest.TestCommon
{
	public static class TestDataLoader
	{
		public static MitamaSetSaveData LoadValidMitamaSet()
		{
			string path = Path.Combine(TestDirectory.MitamaSet, "Valid.mitama.json");
			return SaveDataAccess.LoadMitamaSet(path);
		}
	}
}
