namespace GuiTest.TestUtil
{
	internal sealed class TemporaryDirectory : IDisposable
	{
		public string DirectoryPath { get; }

		public TemporaryDirectory(string category)
		{
			if (string.IsNullOrWhiteSpace(category))
			{
				throw new ArgumentException("Test directory category is empty.", nameof(category));
			}

			DirectoryPath = Path.Combine(
				Path.GetTempPath(),
				"GuiTest",
				category,
				Guid.NewGuid().ToString("N"));

			Directory.CreateDirectory(DirectoryPath);
		}

		public void Dispose()
		{
			if (Directory.Exists(DirectoryPath))
			{
				Directory.Delete(DirectoryPath, true);
			}
		}
	}
}
