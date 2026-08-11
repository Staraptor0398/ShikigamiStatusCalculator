namespace SaveData.Definition
{
	public static class SaveDataVersionDefinition
	{
		public static readonly SaveDataVersion MitamaSet = new SaveDataVersion(SaveDataType.MitamaSet, 2);

		public static readonly SaveDataVersion Build = new SaveDataVersion(SaveDataType.Build, 2);

		public static readonly SaveDataVersion CalculationSnapshot = new SaveDataVersion(SaveDataType.CalculationSnapshot, 2);
	}
}
