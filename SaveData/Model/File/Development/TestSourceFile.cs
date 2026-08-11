namespace SaveData.Model.File.Development
{
	public class TestSourceFile<T>
	{
		public int Version { get; set; }

		public T Data { get; set; }
	}
}
