namespace Gui.Common
{
	public static class SaveDataFileDefinition
	{
		public const string SAVE_DATA_DIRECTORY_NAME = "SaveData";

		public const string BUILD_DIRECTORY_NAME = "Build";
		public const string MITAMA_SET_DIRECTORY_NAME = "MitamaSet";
		public const string SNAPSHOT_DIRECTORY_NAME = "Snapshot";

		public const string BUILD_EXTENSION = ".build.json";
		public const string MITAMA_SET_EXTENSION = ".mitama.json";
		public const string SNAPSHOT_EXTENSION = ".snapshot.json";

		public static readonly string BuildFilter = $"ビルド保存データ (*{BUILD_EXTENSION})|*{BUILD_EXTENSION}";

		public static readonly string MitamaSetFilter = $"御魂セット保存データ (*{MITAMA_SET_EXTENSION})|*{MITAMA_SET_EXTENSION}";

		public static readonly string SnapshotFilter = $"計算結果スナップショット (*{SNAPSHOT_EXTENSION})|*{SNAPSHOT_EXTENSION}";
	}
}
