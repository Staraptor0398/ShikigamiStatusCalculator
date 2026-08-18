using ScenarioRunner.ScenarioFormat;
using System;
using System.IO;

namespace ScenarioRunner.Log
{
	public class ScenarioLogger
	{
		private readonly LogFileWriter mFileWriter;

		public event Action<string> LogWritten;

		public ScenarioLogger(string logDirectoryPath)
		{
			mFileWriter = new LogFileWriter(logDirectoryPath);
		}

		public void Write(string message)
		{
			string log = $"[{DateTime.Now:HH:mm:ss}] {message}";

			mFileWriter.Write(log);
			LogWritten?.Invoke(log);
		}

		public void ScenarioLoaded(Scenario scenario)
		{
			Write($"Scenario loaded: {Path.GetFileName(scenario.FilePath)}");
			Write("Syntax: OK");
			Write($"Steps: {scenario.Steps.Count}");
		}

		public void StepStarted(ScenarioStep step)
		{
			Write($"[{step.LineNumber:D2}] {step.RawText}");
		}

		public void StepPassed(ScenarioStep step)
		{
			Write($"[{step.LineNumber:D2}] {step.RawText}    PASS");
		}

		public void StepFailed(ScenarioStep step, string message)
		{
			Write($"[{step.LineNumber:D2}] {step.RawText}    FAIL");
			Write(message);
		}

		public void Error(string message)
		{
			Write($"ERROR: {message}");
		}
	}
}
