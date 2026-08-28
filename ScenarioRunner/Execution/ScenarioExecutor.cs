using ScenarioRunner.Automation.Watcher;
using ScenarioRunner.Log;
using ScenarioRunner.ScenarioFormat;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace ScenarioRunner.Execution
{
	public class ScenarioExecutor
	{
		private readonly ScenarioCommandExecutor mCommandExecutor;
		private readonly ScenarioLogger mLogger;
		private readonly string mGuiExecutablePath;

		public ScenarioExecutor(ScenarioLogger logger, string guiExecutablePath)
		{
			mLogger = logger;
			mGuiExecutablePath = guiExecutablePath;

			mCommandExecutor = new ScenarioCommandExecutor();
		}

		public ScenarioExecutionResult Execute(Scenario scenario, ScenarioExecutionOptions options)
		{
			var stopwatch = Stopwatch.StartNew();
			var context = new ScenarioExecutonContext(scenario.FilePath, mGuiExecutablePath, options);

			string guiDirectoryPath = Path.GetDirectoryName(mGuiExecutablePath);

			context.ShikigamiDataFilePath = Path.Combine(guiDirectoryPath, "Data", "ShikigamiData.csv");

			string brokenDirectoryPath = Path.Combine(guiDirectoryPath, "Data", "Broken");

			string backupDirectoryPath = Path.Combine(guiDirectoryPath, "Data", "Backup");

			using (var brokenWatcher = new ShikigamiDataFileWatcher(brokenDirectoryPath))
			using (var backupWatcher = new ShikigamiDataFileWatcher(backupDirectoryPath))
			{
				brokenWatcher.FileCreated += path => context.ShikigamiBrokenDataFilePath = path;
				backupWatcher.FileCreated += path => context.ShikigamiBackupDataFilePath = path;

				brokenWatcher.Start();
				backupWatcher.Start();

				int passedCount = 0;

				mLogger.ScenarioStarted(scenario);

				foreach (ScenarioStep step in scenario.Steps)
				{
					try
					{
						mCommandExecutor.Execute(step, context);

						passedCount++;
						mLogger.StepPassed(step);

						if (options.WatchMode)
						{
							Thread.Sleep(500);
						}
					}
					catch (Exception ex)
					{
						stopwatch.Stop();

						mLogger.StepFailed(step, ex.Message);

						var failedResult = new ScenarioExecutionResult(false, passedCount, 1, stopwatch.Elapsed, step.LineNumber, ex.Message);
						mLogger.ScenarioFailed(failedResult);

						return failedResult;
					}
				}


				stopwatch.Stop();

				var result = new ScenarioExecutionResult(true, passedCount, 0, stopwatch.Elapsed, -1, null);
				mLogger.ScenarioPassed(result);

				return result;

			}
		}
	}
}
