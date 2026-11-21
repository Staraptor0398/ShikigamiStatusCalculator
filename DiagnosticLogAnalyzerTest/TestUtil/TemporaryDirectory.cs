using System.Text;

namespace DiagnosticLogAnalyzerTest.TestUtil
{
	internal sealed class TemporaryDirectory : IDisposable
	{
		public string Path { get; }

		public TemporaryDirectory()
		{
			Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "DiagnosticLogAnalyzerTest", Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(Path);
		}

		public string Write(string relativePath, string contents)
		{
			string path = System.IO.Path.Combine(Path, relativePath.Replace('/', System.IO.Path.DirectorySeparatorChar));
			Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
			File.WriteAllText(path, contents, new UTF8Encoding(false));
			return path;
		}

		public void Dispose()
		{
			if (Directory.Exists(Path))
			{
				Directory.Delete(Path, true);
			}
		}
	}
}
