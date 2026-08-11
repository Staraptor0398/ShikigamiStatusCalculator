namespace SaveData.Model.File
{
	public class SaveDataFile<T>
	{
		public int Version { get; set; }

		public T Data { get; set; }
	}
}
