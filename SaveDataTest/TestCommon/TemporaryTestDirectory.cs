using System;
using System.IO;
using System.Text;

namespace SaveDataTest.TestCommon
{
	public sealed class TemporaryTestDirectory : IDisposable
	{
		public string DirectoryPath { get; }

		public TemporaryTestDirectory()
		{
			DirectoryPath = Path.Combine(Path.GetTempPath(), "ShikigamiStatusCalculator", "SaveDataTest", Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(DirectoryPath);
		}

		public string GetFilePath(string fileName)
		{
			return Path.Combine(DirectoryPath, fileName);
		}

		public string WriteFile(string fileName, string content)
		{
			string filePath = GetFilePath(fileName);
			File.WriteAllText(filePath, content, new UTF8Encoding(false));
			return filePath;
		}

		public void Dispose()
		{
			if (!Directory.Exists(DirectoryPath))
			{
				return;
			}

			Directory.Delete(DirectoryPath, true);
		}
	}
}
