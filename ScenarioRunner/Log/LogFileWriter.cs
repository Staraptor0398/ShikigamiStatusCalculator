using System;
using System.IO;

namespace ScenarioRunner.Log
{
	public class LogFileWriter
	{
		private readonly string mLogFilePath;

		public LogFileWriter(string logDirectoryPath)
		{
			Directory.CreateDirectory(logDirectoryPath);
			mLogFilePath = Path.Combine(logDirectoryPath, $"ScenarioRunner_{DateTime.Now:yyyy-MM-dd}.log");
		}

		public void Write(string message)
		{
			File.AppendAllText(mLogFilePath, message);
		}
	}
}
