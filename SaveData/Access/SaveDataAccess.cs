using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SaveData.Definition;
using SaveData.Migration;
using SaveData.Model;
using SaveData.Model.File;
using System;
using System.IO;
using System.Text;

namespace SaveData.Access
{
	public static class SaveDataAccess
	{
		public static void SaveMitamaSet(string path, MitamaSetSaveData data)
		{
			save(path, data, SaveDataVersionDefinition.MitamaSet);
		}

		public static void SaveBuild(string path, BuildSaveData data)
		{
			save(path, data, SaveDataVersionDefinition.Build);
		}

		public static void SaveSnapshot(string path, CalculationSnapshotSaveData data)
		{
			save(path, data, SaveDataVersionDefinition.CalculationSnapshot);
		}

		public static MitamaSetSaveData LoadMitamaSet(string path)
		{
			return load<MitamaSetSaveData>(path, SaveDataVersionDefinition.MitamaSet);
		}

		public static BuildSaveData LoadBuild(string path)
		{
			return load<BuildSaveData>(path, SaveDataVersionDefinition.Build);
		}

		public static CalculationSnapshotSaveData LoadSnapshot(string path)
		{
			return load<CalculationSnapshotSaveData>(path, SaveDataVersionDefinition.CalculationSnapshot);
		}

		private static void save<T>(string path, T data, SaveDataVersion saveDataVersion)
		{
			SaveDataFile<T> file = new SaveDataFile<T>
			{
				Version = saveDataVersion.Version,
				Data = data
			};

			string json = JsonConvert.SerializeObject(file, Formatting.Indented);

			File.WriteAllText(path, json, new UTF8Encoding(false));
		}

		private static T load<T>(string path, SaveDataVersion currentVersion)
		{
			string json = File.ReadAllText(path, Encoding.UTF8);

			JObject root = JObject.Parse(json);

			int sourceVersion = getSourceVersion(root);

			if (sourceVersion > currentVersion.Version)
			{
				throw new NotSupportedException($"Unsupported save data version: {sourceVersion}");
			}

			if (sourceVersion < currentVersion.Version)
			{
				root = SaveDataMigrator.Migrate(root, currentVersion, sourceVersion);
			}

			SaveDataFile<T> file =
			root.ToObject<SaveDataFile<T>>();

			if (file == null)
			{
				return default;
			}

			return file.Data;
		}

		private static int getSourceVersion(JObject root)
		{
			JToken versionToken = root["Version"];

			// Version導入前のSaveDataはv1として扱う。
			if (versionToken == null)
			{
				return 1;
			}

			return versionToken.Value<int>();
		}
	}
}
