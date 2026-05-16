namespace Gui.Common
{
	public static class SaveDataFileDefinition
	{
		public const string SaveDataDirectoryName = "SaveData";

		public const string BuildDirectoryName = "Build";
		public const string MitamaSetDirectoryName = "MitamaSet";
		public const string SnapshotDirectoryName = "Snapshot";

		public const string BuildExtension = ".build.json";
		public const string MitamaSetExtension = ".mitama.json";
		public const string SnapshotExtension = ".snapshot.json";

		public static readonly string BuildFilter = $"ビルド保存データ (*{BuildExtension})|*{BuildExtension}";

		public static readonly string MitamaSetFilter = $"御魂セット保存データ (*{MitamaSetExtension})|*{MitamaSetExtension}";

		public static readonly string SnapshotFilter = $"計算結果スナップショット (*{SnapshotExtension})|*{SnapshotExtension}";
	}
}
