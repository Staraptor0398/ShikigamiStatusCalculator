using System;
using System.IO;
using System.Text;

namespace ScenarioRunner.Log
{
	public class LogFileWriter : IDisposable
	{
		private readonly StreamWriter mWriter;

		private bool mDisposed;

		public LogFileWriter(string logDirectoryPath, string scenarioFilePath)
		{
			Directory.CreateDirectory(logDirectoryPath);

			string scenarioName = Path.GetFileNameWithoutExtension(scenarioFilePath);
			string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
			string logFileName = $"ScenarioRunner_{timestamp}_{scenarioName}";

			int suffix = 1;

			while (true)
			{
				string fileName = suffix == 1 ? $"{logFileName}.log" : $"{logFileName}_{suffix}.log";

				string logFilePath = Path.Combine(logDirectoryPath, fileName);

				try
				{
					var stream = new FileStream(logFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.Read);

					mWriter = new StreamWriter(stream, Encoding.UTF8)
					{
						AutoFlush = true
					};

					break;
				}
				catch (IOException) when (File.Exists(logFilePath))
				{
					suffix++;
				}
			}
		}

		public void Write(string message)
		{
			if (mDisposed)
			{
				throw new ObjectDisposedException(nameof(LogFileWriter));
			}

			mWriter.WriteLine(message);
		}

		public void Dispose()
		{
			if (mDisposed)
			{
				return;
			}

			mWriter.Dispose();
			mDisposed = true;
		}
	}
}
