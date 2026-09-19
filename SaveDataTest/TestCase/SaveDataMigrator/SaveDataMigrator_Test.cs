using Newtonsoft.Json.Linq;
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
			JObject root = JObject.Parse(SaveDataTestData.V1_MITAMA_SET_JSON);

			JObject migrated = SaveDataMigrator.Migrate(root, SaveDataVersionDefinition.MitamaSet, 1);
			SaveDataFile<MitamaSetSaveData> actualFile = migrated.ToObject<SaveDataFile<MitamaSetSaveData>>();

			Assert.IsNotNull(actualFile);
			Assert.AreEqual(2, actualFile.Version);

			MitamaSetSaveData expected = SaveDataTestData.CreateMitamaSet();

			TestAssert.AreEqual(expected, actualFile.Data);
		}

		[TestMethod]
		public void Migrate_NullRoot()
		{
			JObject actual = SaveDataMigrator.Migrate(null, SaveDataVersionDefinition.MitamaSet, 1);

			Assert.IsNull(actual);
		}

		[TestMethod]
		public void Migrate_NullTargetVersion()
		{
			JObject root = JObject.Parse(SaveDataTestData.V1_MITAMA_SET_JSON);

			bool exceptionThrown = false;

			try
			{
				SaveDataMigrator.Migrate(root, null, 1);
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
			JObject root = JObject.Parse(SaveDataTestData.V1_MITAMA_SET_JSON);

			bool exceptionThrown = false;

			try
			{
				SaveDataMigrator.Migrate(root, SaveDataVersionDefinition.MitamaSet, 3);
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
			JObject root = JObject.Parse(SaveDataTestData.V1_MITAMA_SET_JSON);

			bool exceptionThrown = false;

			try
			{
				SaveDataMigrator.Migrate(root, SaveDataVersionDefinition.MitamaSet, 0);
			}
			catch (NotSupportedException)
			{
				exceptionThrown = true;
			}

			Assert.IsTrue(exceptionThrown);
		}
	}
}
