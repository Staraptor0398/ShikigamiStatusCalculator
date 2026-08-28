using ScenarioRunner.Execution;
using ScenarioRunner.ScenarioFormat;
using System;
using System.IO;

namespace ScenarioRunner.Log
{
	public class ScenarioLogger
	{
		private readonly LogFileWriter mFileWriter;

		public event Action<string> LogWritten;
		public event Action<ScenarioStep> StepStartedEvent;
		public event Action<ScenarioStep> StepPassedEvent;
		public event Action<ScenarioStep> StepFailedEvent;

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
			Write($"[{step.LineNumber:D2}] {step.RawText}    START");

			StepStartedEvent?.Invoke(step);
		}

		public void StepPassed(ScenarioStep step)
		{
			Write($"[{step.LineNumber:D2}] {step.RawText}    PASS");

			StepPassedEvent?.Invoke(step);
		}

		public void StepFailed(ScenarioStep step, string message)
		{
			Write($"[{step.LineNumber:D2}] {step.RawText}    FAIL");
			Write(message);

			StepFailedEvent?.Invoke(step);
		}

		public void Error(string message)
		{
			Write($"ERROR: {message}");
		}
		public void ScenarioStarted(Scenario scenario)
		{
			Write("========================================");
			Write($"Scenario started: {Path.GetFileName(scenario.FilePath)}");
			Write("========================================");
		}
		public void ScenarioPassed(ScenarioExecutionResult result)
		{
			Write($"Scenario Result: PASS");
			Write($"Passed: {result.PassedCount}");
			Write($"Elapsed: {result.Elapsed.TotalSeconds:F2} sec");
		}
		public void ScenarioFailed(ScenarioExecutionResult result)
		{
			Write($"Scenario Result: FAIL");
			Write($"Failed line: {result.FailedLineNumber}");
			Write($"Elapsed: {result.Elapsed.TotalSeconds:F2} sec");
		}
	}
}
