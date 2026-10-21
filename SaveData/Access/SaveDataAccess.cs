using SaveData.Definition;
using SaveData.Migration;
using SaveData.Model;
using SaveData.Model.File;
using System;

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

		public static MitamaSetSaveData LoadMitamaSet(string path, Func<string, IDisposable> track = null)
		{
			return load<MitamaSetSaveData>(path, SaveDataVersionDefinition.MitamaSet, track);
		}

		public static BuildSaveData LoadBuild(string path, Func<string, IDisposable> track = null)
		{
			return load<BuildSaveData>(path, SaveDataVersionDefinition.Build, track);
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

			JsonDataAccess.Save(path, file);
		}

		private static T load<T>(string path, SaveDataVersion currentVersion, Func<string, IDisposable> track = null)
		{
			string json = JsonDataAccess.ReadText(path, track);
			int sourceVersion;

			using (track?.Invoke("SaveData.VersionCheck"))
			{
				sourceVersion = JsonDataAccess.GetVersion(json);

				if (sourceVersion > currentVersion.Version)
				{
					throw new NotSupportedException($"Unsupported save data version: {sourceVersion}");
				}
			}

			if (sourceVersion < currentVersion.Version)
			{
				using (track?.Invoke("SaveData.Migrate"))
				{
					json = SaveDataMigrator.Migrate(json, currentVersion, sourceVersion);
				}
			}

			SaveDataFile<T> file = JsonDataAccess.Deserialize<SaveDataFile<T>>(json, track);
			return file == null ? default : file.Data;
		}
	}
}
