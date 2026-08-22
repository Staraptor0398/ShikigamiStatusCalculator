using System;
using System.IO;
using System.Text;

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
			File.AppendAllText(mLogFilePath, message + Environment.NewLine, Encoding.UTF8);
		}
	}
}
