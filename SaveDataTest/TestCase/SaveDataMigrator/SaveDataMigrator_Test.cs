using SaveData.Definition;
using SaveData.Migration;
using SaveData.Model;
using SaveData.Model.File;
using SaveDataTest.TestCommon;
using System.Text.Json;

namespace SaveDataTest.TestCase
{
	[TestClass]
	public class SaveDataMigrator_Test
	{
		[TestMethod]
		public void MigrateMitamaSet_V1ToV2()
		{
			string json = SaveDataTestData.V1_MITAMA_SET_JSON;

			string migrated = SaveDataMigrator.Migrate(json, SaveDataVersionDefinition.MitamaSet, 1);
			SaveDataFile<MitamaSetSaveData> actualFile = JsonSerializer.Deserialize<SaveDataFile<MitamaSetSaveData>>(migrated);

			Assert.IsNotNull(actualFile);
			Assert.AreEqual(2, actualFile.Version);

			MitamaSetSaveData expected = SaveDataTestData.CreateMitamaSet();

			TestAssert.AreEqual(expected, actualFile.Data);
		}

		[TestMethod]
		public void Migrate_NullRoot()
		{
			string actual = SaveDataMigrator.Migrate(null, SaveDataVersionDefinition.MitamaSet, 1);

			Assert.IsNull(actual);
		}

		[TestMethod]
		public void Migrate_NullTargetVersion()
		{
			string json = SaveDataTestData.V1_MITAMA_SET_JSON;

			bool exceptionThrown = false;

			try
			{
				SaveDataMigrator.Migrate(json, null, 1);
			}
			catch (ArgumentNullException)
			{
				exceptionThrown = true;
			}

			Assert.IsTrue(exceptionThrown);
		}

		[TestMethod]
		public void Migrate_Downgrade()
		{
			string json = SaveDataTestData.V1_MITAMA_SET_JSON;

			bool exceptionThrown = false;

			try
			{
				SaveDataMigrator.Migrate(json, SaveDataVersionDefinition.MitamaSet, 3);
			}
			catch (NotSupportedException)
			{
				exceptionThrown = true;
			}

			Assert.IsTrue(exceptionThrown);
		}

		[TestMethod]
		public void Migrate_UnsupportedSourceVersion()
		{
			string json = SaveDataTestData.V1_MITAMA_SET_JSON;

			bool exceptionThrown = false;

			try
			{
				SaveDataMigrator.Migrate(json, SaveDataVersionDefinition.MitamaSet, 0);
			}
			catch (NotSupportedException)
			{
				exceptionThrown = true;
			}

			Assert.IsTrue(exceptionThrown);
		}
	}
}
