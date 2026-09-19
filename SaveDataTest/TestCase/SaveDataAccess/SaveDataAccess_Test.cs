using SaveData.Access;
using SaveData.Model;
using SaveDataTest.TestCommon;
using System;

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
				assertMitamaSet(SaveDataTestData.CreateMitamaSet(), actual);
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

				Assert.IsNotNull(actual);
				Assert.AreEqual(expected.ShikigamiName, actual.ShikigamiName);
				assertMitamaSet(expected.MitamaSet, actual.MitamaSet);
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

				Assert.IsNotNull(actual);
				Assert.AreEqual(expected.SnapshotName, actual.SnapshotName);
				Assert.AreEqual(expected.CreatedAt, actual.CreatedAt);
				Assert.AreEqual(expected.ShikigamiName, actual.ShikigamiName);
				assertMitamaSet(expected.MitamaSet, actual.MitamaSet);
				assertStatus(expected.MitamaStatus, actual.MitamaStatus);
				assertStatus(expected.FinalStatus, actual.FinalStatus);
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

				assertMitamaSet(expected, actual);
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

				Assert.IsNotNull(actual);
				Assert.AreEqual(expected.ShikigamiName, actual.ShikigamiName);
				assertMitamaSet(expected.MitamaSet, actual.MitamaSet);
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

				Assert.IsNotNull(actual);
				Assert.AreEqual(expected.SnapshotName, actual.SnapshotName);
				Assert.AreEqual(expected.CreatedAt, actual.CreatedAt);
				Assert.AreEqual(expected.ShikigamiName, actual.ShikigamiName);
				assertMitamaSet(expected.MitamaSet, actual.MitamaSet);
				assertStatus(expected.MitamaStatus, actual.MitamaStatus);
				assertStatus(expected.FinalStatus, actual.FinalStatus);
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

		private static void assertMitamaSet(MitamaSetSaveData expected, MitamaSetSaveData actual)
		{
			Assert.IsNotNull(expected);
			Assert.IsNotNull(actual);

			Assert.AreEqual(expected.Mitamas.Count, actual.Mitamas.Count);

			for (int i = 0; i < expected.Mitamas.Count; i++)
			{
				assertMitama(expected.Mitamas[i], actual.Mitamas[i]);
			}

			Assert.AreEqual(expected.SetEffects.Count, actual.SetEffects.Count);

			for (int i = 0; i < expected.SetEffects.Count; i++)
			{
				assertSetEffect(expected.SetEffects[i], actual.SetEffects[i]);
			}

			Assert.AreEqual(expected.UniqueEffects.Count, actual.UniqueEffects.Count);

			for (int i = 0; i < expected.UniqueEffects.Count; i++)
			{
				assertSetEffect(expected.UniqueEffects[i], actual.UniqueEffects[i]);
			}
		}

		private static void assertMitama(MitamaSaveData expected, MitamaSaveData actual)
		{
			Assert.IsNotNull(expected);
			Assert.IsNotNull(actual);

			Assert.AreEqual(expected.Slot, actual.Slot);
			assertStatValue(expected.MainStat, actual.MainStat);

			Assert.AreEqual(expected.SubStats.Count, actual.SubStats.Count);

			for (int i = 0; i < expected.SubStats.Count; i++)
			{
				assertStatValue(expected.SubStats[i], actual.SubStats[i]);
			}
		}

		private static void assertSetEffect(SetEffectSaveData expected, SetEffectSaveData actual)
		{
			Assert.IsNotNull(expected);
			Assert.IsNotNull(actual);
			assertStatValue(expected.Stat, actual.Stat);
		}

		private static void assertStatValue(StatValueSaveData expected, StatValueSaveData actual)
		{
			Assert.IsNotNull(expected);
			Assert.IsNotNull(actual);

			Assert.AreEqual(expected.Type, actual.Type);
			Assert.AreEqual(expected.Value, actual.Value);
		}

		private static void assertStatus(StatusSaveData expected, StatusSaveData actual)
		{
			Assert.IsNotNull(expected);
			Assert.IsNotNull(actual);

			Assert.AreEqual(expected.Attack, actual.Attack);
			Assert.AreEqual(expected.HP, actual.HP);
			Assert.AreEqual(expected.Defense, actual.Defense);
			Assert.AreEqual(expected.Speed, actual.Speed);
			Assert.AreEqual(expected.CritRate, actual.CritRate);
			Assert.AreEqual(expected.CritDamage, actual.CritDamage);
			Assert.AreEqual(expected.EffectHit, actual.EffectHit);
			Assert.AreEqual(expected.EffectResist, actual.EffectResist);
		}
	}
}
