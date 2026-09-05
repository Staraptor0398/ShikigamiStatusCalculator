using ScenarioRunner.Execution;
using ScenarioRunner.ScenarioFormat;
using System;
using System.IO;

namespace ScenarioRunner.Log
{
	public class ScenarioLogger
	{
		private readonly string mLogDirectoryPath;
		private LogFileWriter mFileWriter;

		public event Action<string> LogWritten;
		public event Action<ScenarioStep> StepStartedEvent;
		public event Action<ScenarioStep> StepPassedEvent;
		public event Action<ScenarioStep> StepFailedEvent;

		public ScenarioLogger(string logDirectoryPath)
		{
			mLogDirectoryPath = logDirectoryPath;
		}

		public void Write(string message)
		{
			string log = $"[{DateTime.Now:HH:mm:ss}] {message}";

			mFileWriter.Write(log);
			LogWritten?.Invoke(log);
		}

		public void ScenarioValidationStarted(string filePath)
		{
			mFileWriter = new LogFileWriter(mLogDirectoryPath);

			Write("========================================");
			Write($"Scenario: {Path.GetFileName(filePath)}");
			Write("========================================");
			Write("");
			Write("[Validation]");
		}

		public void ScenarioValidationFailed(string message)
		{
			Write("Validation: FAILED");
			Write($"Error: {message}");
			Write("");
			Write("[Result]");
			Write("Scenario Result: VALIDATION FAILED");
		}

		public void ScenarioLoaded(Scenario scenario)
		{
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
			Write("");
			Write("[Execution]");
		}

		public void ScenarioStopped(ScenarioExecutionResult result)
		{
			Write("");
			Write("[Result]");
			Write("Scenario Result: STOPPED");
			Write($"Passed: {result.PassedCount}");
			Write($"Elapsed: {result.Elapsed.TotalSeconds:F2} sec");
		}

		public void ScenarioPassed(ScenarioExecutionResult result)
		{
			Write("");
			Write("[Result]");
			Write("Scenario Result: PASS");
			Write($"Passed: {result.PassedCount}");
			Write($"Elapsed: {result.Elapsed.TotalSeconds:F2} sec");
		}

		public void ScenarioFailed(ScenarioExecutionResult result)
		{
			Write("");
			Write("[Result]");
			Write("Scenario Result: FAIL");
			Write($"Failed line: {result.FailedLineNumber}");
			Write($"Elapsed: {result.Elapsed.TotalSeconds:F2} sec");
		}
	}
}
