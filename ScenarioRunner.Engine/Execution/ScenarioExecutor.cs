using ScenarioRunner.Automation.Capture;
using ScenarioRunner.Automation.Model;
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
		private readonly ScreenshotCapturer mScreenshotCapturer;

		private readonly string mGuiExecutablePath;

		private readonly WindowBounds mGuiBounds;

		private CancellationTokenSource mCancellationTokenSource;

		public ScenarioExecutor(ScenarioLogger logger, string guiExecutablePath, WindowBounds guiBounds)
		{
			mLogger = logger;
			mGuiExecutablePath = guiExecutablePath;
			mGuiBounds = guiBounds;

			mCommandExecutor = new ScenarioCommandExecutor();

			string captureDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Capture");
			mScreenshotCapturer = new ScreenshotCapturer(captureDirectoryPath);
		}

		public ScenarioExecutionResult Execute(Scenario scenario, ScenarioExecutionOptions options)
		{
			var stopwatch = Stopwatch.StartNew();

			mCancellationTokenSource = new CancellationTokenSource();

			var context = new ScenarioExecutonContext(scenario.FilePath, mGuiExecutablePath, options, mGuiBounds, mCancellationTokenSource.Token);

			try
			{
				string guiDirectoryPath = Path.GetDirectoryName(mGuiExecutablePath);

				context.ShikigamiDataFilePath = Path.Combine(guiDirectoryPath, "Data", "ShikigamiData.csv");

				string brokenDirectoryPath = Path.Combine(guiDirectoryPath, "Data", "Broken");
				string backupDirectoryPath = Path.Combine(guiDirectoryPath, "Data", "Backup");

				Directory.CreateDirectory(brokenDirectoryPath);
				Directory.CreateDirectory(backupDirectoryPath);

				using (var brokenWatcher = requiresBrokenWatcher(scenario) ? createAndStartWatcher(brokenDirectoryPath, path => context.ShikigamiBrokenDataFilePath = path) : null)
				using (var backupWatcher = requiresBackupWatcher(scenario) ? createAndStartWatcher(backupDirectoryPath, path => context.ShikigamiBackupDataFilePath = path) : null)
				{
					int passedCount = 0;

					mLogger.ScenarioStarted(scenario);

					foreach (ScenarioStep step in scenario.Steps)
					{
						try
						{
							context.CancellationToken.ThrowIfCancellationRequested();

							mLogger.StepStarted(step);

							mCommandExecutor.Execute(step, context);

							passedCount++;
							mLogger.StepPassed(step);

							if (options.WatchMode)
							{
								Thread.Sleep(500);
							}
						}
						catch (OperationCanceledException)
						{
							stopwatch.Stop();

							var stoppedResult = new ScenarioExecutionResult(false, true, passedCount, 0, stopwatch.Elapsed, -1, null);
							mLogger.ScenarioStopped(stoppedResult);

							return stoppedResult;
						}
						catch (Exception ex)
						{
							stopwatch.Stop();

							string screenshotPath = tryCaptureFailureScreenshot(scenario.FilePath, step.LineNumber, out string screenshotError);

							mLogger.StepFailed(step, ex.Message);

							if (!string.IsNullOrWhiteSpace(screenshotPath))
							{
								mLogger.Write($"Screenshot: {screenshotPath}");
							}
							else if (!string.IsNullOrWhiteSpace(screenshotError))
							{
								mLogger.Write($"Screenshot capture failed: {screenshotError}");
							}

							var failedResult = new ScenarioExecutionResult(false, false, passedCount, 1, stopwatch.Elapsed, step.LineNumber, ex.Message);
							mLogger.ScenarioFailed(failedResult);

							return failedResult;
						}
					}

					stopwatch.Stop();

					var result = new ScenarioExecutionResult(true, false, passedCount, 0, stopwatch.Elapsed, -1, null);
					mLogger.ScenarioPassed(result);

					return result;
				}
			}
			finally
			{
				if (options.CleanupGuiOnExit)
				{
					cleanupGui(context);
				}
			}
		}

		public void Stop()
		{
			mCancellationTokenSource?.Cancel();
		}

		private static bool requiresBrokenWatcher(Scenario scenario)
		{
			foreach (ScenarioStep step in scenario.Steps)
			{
				if (step.CommandType == ScenarioCommandType.WAIT_SHIKIGAMI_AUTO_REPAIR)
				{
					return true;
				}

				if (step.CommandType == ScenarioCommandType.RECOVER_SHIKIGAMI && step.Arguments[0] == "BROKEN")
				{
					return true;
				}
			}

			return false;
		}

		private static bool requiresBackupWatcher(Scenario scenario)
		{
			foreach (ScenarioStep step in scenario.Steps)
			{
				if (step.CommandType == ScenarioCommandType.RECOVER_SHIKIGAMI && step.Arguments[0] == "BACKUP")
				{
					return true;
				}
			}

			return false;
		}

		private static ShikigamiDataFileWatcher createAndStartWatcher(string directoryPath, Action<string> onFileCreated)
		{
			var watcher = new ShikigamiDataFileWatcher(directoryPath);

			try
			{
				watcher.FileCreated += onFileCreated;
				watcher.Start();

				return watcher;
			}
			catch
			{
				watcher.Dispose();
				throw;
			}
		}

		private string tryCaptureFailureScreenshot(string scenarioPath, int lineNumber, out string errorMessage)
		{
			try
			{
				errorMessage = null;

				return mScreenshotCapturer.Capture(scenarioPath, lineNumber);
			}
			catch (Exception ex)
			{
				errorMessage = ex.Message;

				return null;
			}
		}

		private static void cleanupGui(ScenarioExecutonContext context)
		{
			if (context.GuiSession == null)
			{
				return;
			}

			var session = context.GuiSession;

			try
			{
				try
				{
					session.Application.Close();
				}
				catch
				{
					session.Application.Kill();
				}

				if (!session.Application.HasExited)
				{
					session.Application.Kill();
				}

				if (!session.Application.HasExited)
				{
					throw new InvalidOperationException("Gui.exe could not be terminated.");
				}
			}
			finally
			{
				context.GuiSession = null;
				session.Dispose();
			}
		}
	}
}
