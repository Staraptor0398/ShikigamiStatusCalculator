using System;
using System.IO;
using System.Text;

namespace ScenarioRunner.Log
{
	public class LogFileWriter : IDisposable
	{
		private readonly StreamWriter mWriter;

		private bool mDisposed;

		public LogFileWriter(string logDirectoryPath)
		{
			Directory.CreateDirectory(logDirectoryPath);

			string logFilePath = Path.Combine(logDirectoryPath, $"ScenarioRunner_{DateTime.Now:yyyyMMdd_HHmmss}.log");

			mWriter = new StreamWriter(logFilePath, true, Encoding.UTF8)
			{
				AutoFlush = true
			};
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
