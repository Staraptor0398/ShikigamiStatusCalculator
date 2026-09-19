using ScenarioRunner.Automation.Layout;
using ScenarioRunner.Automation.Model;
using ScenarioRunner.Execution;
using ScenarioRunner.Log;
using ScenarioRunner.ScenarioFormat;
using ScenarioRunner.Startup;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ScenarioRunnerCli
{
	public class ScenarioCliRunner
	{
		public bool Run(string scenarioPath, string guiExecutablePath)
		{
			if (string.IsNullOrWhiteSpace(scenarioPath))
			{
				throw new ArgumentException("Scenario file path is empty.", nameof(scenarioPath));
			}

			string resolvedScenarioPath = Path.GetFullPath(scenarioPath);
			string resolvedGuiExecutablePath = GuiExecutablePathResolver.Resolve(guiExecutablePath);
			string logDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");

			var logger = new ScenarioLogger(logDirectoryPath);
			logger.LogWritten += Console.WriteLine;

			try
			{
				logger.ScenarioValidationStarted(resolvedScenarioPath);

				var loader = new ScenarioLoader();
				Scenario scenario = loader.Load(resolvedScenarioPath);

				logger.ScenarioLoaded(scenario);

				WindowBounds guiBounds = createGuiBounds();
				var executor = new ScenarioExecutor(logger, resolvedGuiExecutablePath, guiBounds);
				var executionOptions = new ScenarioExecutionOptions(false);
				ScenarioExecutionResult result = executor.Execute(scenario, executionOptions);

				return result.IsSuccess;
			}
			catch (ScenarioValidationException ex)
			{
				logger.ScenarioValidationFailed(ex.Message);
				return false;
			}
			catch (Exception ex)
			{
				logger.Error(ex.Message);
				return false;
			}
		}

		private static WindowBounds createGuiBounds()
		{
			Screen primaryScreen = Screen.PrimaryScreen;

			if (primaryScreen == null)
			{
				throw new InvalidOperationException("Primary screen could not be resolved.");
			}

			Rectangle workingArea = primaryScreen.WorkingArea;
			var workingAreaBounds = new WindowBounds(workingArea.X, workingArea.Y, workingArea.Width, workingArea.Height);
			var windowLayout = new ScenarioWindowLayout(workingAreaBounds);

			return windowLayout.GuiBounds;
		}
	}
}
