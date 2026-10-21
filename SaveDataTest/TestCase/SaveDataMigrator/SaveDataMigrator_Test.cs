using SaveData.Access;
using SaveData.Definition;
using SaveData.Migration;
using SaveData.Model;
using SaveData.Model.File;
using SaveDataTest.TestCommon;

namespace SaveDataTest.TestCase
{
	[TestClass]
	public class SaveDataMigrator_Test
	{
		[TestMethod]
		public void MigrateMitamaSet_V1ToV2()
		{
			string migrated = SaveDataMigrator.Migrate(SaveDataTestData.V1_MITAMA_SET_JSON, SaveDataVersionDefinition.MitamaSet, 1);

			SaveDataFile<MitamaSetSaveData> actualFile = JsonDataAccess.Deserialize<SaveDataFile<MitamaSetSaveData>>(migrated);

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
			bool exceptionThrown = false;

			try
			{
				SaveDataMigrator.Migrate(SaveDataTestData.V1_MITAMA_SET_JSON, null, 1);
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
			bool exceptionThrown = false;

			try
			{
				SaveDataMigrator.Migrate(SaveDataTestData.V1_MITAMA_SET_JSON, SaveDataVersionDefinition.MitamaSet, 3);
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
			bool exceptionThrown = false;

			try
			{
				SaveDataMigrator.Migrate(SaveDataTestData.V1_MITAMA_SET_JSON, SaveDataVersionDefinition.MitamaSet, 0);
			}
			catch (NotSupportedException)
			{
				exceptionThrown = true;
			}

			Assert.IsTrue(exceptionThrown);
		}
	}
}
