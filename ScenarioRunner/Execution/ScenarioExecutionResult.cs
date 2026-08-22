using System;

namespace ScenarioRunner.Execution
{
	public class ScenarioExecutionResult
	{
		public bool IsSuccess { get; }
		public int PassedCount { get; }
		public int FailedCount { get; }
		public TimeSpan Elapsed { get; }
		public int FailedLineNumber { get; }
		public string ErrorMessage { get; }

		public ScenarioExecutionResult(bool isSuccess, int passedCount, int failedCount, TimeSpan elapsed, int failedLineNumber, string errorMessage)
		{
			IsSuccess = isSuccess;
			PassedCount = passedCount;
			FailedCount = failedCount;
			Elapsed = elapsed;
			FailedLineNumber = failedLineNumber;
			ErrorMessage = errorMessage;
		}
	}
}
