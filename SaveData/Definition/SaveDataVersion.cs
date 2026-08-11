namespace SaveData.Definition
{
	public sealed class SaveDataVersion
	{
		public SaveDataType Type { get; }
		public int Version { get; }
		public SaveDataVersion(
		SaveDataType type,
		int version)
		{
			Type = type;
			Version = version;
		}
	}
}
