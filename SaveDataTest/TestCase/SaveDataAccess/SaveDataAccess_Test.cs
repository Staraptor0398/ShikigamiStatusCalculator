using SaveData.Access;
using SaveData.Model;
using SaveDataTest.TestCommon;

namespace SaveDataTest.TestCase
{
	[TestClass]
	public class SaveDataAccess_Test
	{
		[TestMethod]
		public void LoadMitamaSet_V1()
		{
			using (var directory = new TemporaryTestDirectory())
			{
				string filePath = directory.WriteFile("V1.mitama.json", SaveDataTestData.V1_MITAMA_SET_JSON);

				MitamaSetSaveData actual = SaveDataAccess.LoadMitamaSet(filePath);
				MitamaSetSaveData expected = SaveDataTestData.CreateMitamaSet();

				TestAssert.AreEqual(expected, actual);
			}
		}

		[TestMethod]
		public void LoadBuild_V1()
		{
			using (var directory = new TemporaryTestDirectory())
			{
				string filePath = directory.WriteFile("V1.build.json", SaveDataTestData.V1_BUILD_JSON);

				BuildSaveData actual = SaveDataAccess.LoadBuild(filePath);
				BuildSaveData expected = SaveDataTestData.CreateBuild();

				TestAssert.AreEqual(expected, actual);
			}
		}

		[TestMethod]
		public void LoadSnapshot_V1()
		{
			using (var directory = new TemporaryTestDirectory())
			{
				string filePath = directory.WriteFile("V1.snapshot.json", SaveDataTestData.V1_SNAPSHOT_JSON);

				CalculationSnapshotSaveData actual = SaveDataAccess.LoadSnapshot(filePath);
				CalculationSnapshotSaveData expected = SaveDataTestData.CreateSnapshot();

				TestAssert.AreEqual(expected, actual);
			}
		}

		[TestMethod]
		public void SaveLoadMitamaSet_CurrentVersion()
		{
			using (var directory = new TemporaryTestDirectory())
			{
				string filePath = directory.GetFilePath("Current.mitama.json");
				MitamaSetSaveData expected = SaveDataTestData.CreateMitamaSet();

				SaveDataAccess.SaveMitamaSet(filePath, expected);
				MitamaSetSaveData actual = SaveDataAccess.LoadMitamaSet(filePath);

				TestAssert.AreEqual(expected, actual);
			}
		}

		[TestMethod]
		public void SaveLoadBuild_CurrentVersion()
		{
			using (var directory = new TemporaryTestDirectory())
			{
				string filePath = directory.GetFilePath("Current.build.json");
				BuildSaveData expected = SaveDataTestData.CreateBuild();

				SaveDataAccess.SaveBuild(filePath, expected);
				BuildSaveData actual = SaveDataAccess.LoadBuild(filePath);

				TestAssert.AreEqual(expected, actual);
			}
		}

		[TestMethod]
		public void SaveLoadSnapshot_CurrentVersion()
		{
			using (var directory = new TemporaryTestDirectory())
			{
				string filePath = directory.GetFilePath("Current.snapshot.json");
				CalculationSnapshotSaveData expected = SaveDataTestData.CreateSnapshot();

				SaveDataAccess.SaveSnapshot(filePath, expected);
				CalculationSnapshotSaveData actual = SaveDataAccess.LoadSnapshot(filePath);

				TestAssert.AreEqual(expected, actual);
			}
		}

		[TestMethod]
		public void LoadMitamaSet_FutureVersion()
		{
			using (var directory = new TemporaryTestDirectory())
			{
				string filePath = directory.WriteFile("Future.mitama.json", SaveDataTestData.FUTURE_VERSION_MITAMA_SET_JSON);

				bool exceptionThrown = false;

				try
				{
					SaveDataAccess.LoadMitamaSet(filePath);
				}
				catch (NotSupportedException)
				{
					exceptionThrown = true;
				}

				Assert.IsTrue(exceptionThrown);
			}
		}
	}
}
