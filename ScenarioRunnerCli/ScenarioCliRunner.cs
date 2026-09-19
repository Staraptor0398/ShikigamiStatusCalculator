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
		public bool Run(CommandLineOptions options)
		{
			if (options == null)
			{
				throw new ArgumentNullException(nameof(options));
			}

			string scenarioPath = Path.GetFullPath(options.ScenarioPath);
			string guiExecutablePath = GuiExecutablePathResolver.Resolve(options.GuiExecutablePath);
			string logDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");

			var logger = new ScenarioLogger(logDirectoryPath);
			logger.LogWritten += Console.WriteLine;

			try
			{
				logger.ScenarioValidationStarted(scenarioPath);

				var loader = new ScenarioLoader();
				Scenario scenario = loader.Load(scenarioPath);

				logger.ScenarioLoaded(scenario);

				WindowBounds guiBounds = createGuiBounds();
				var executor = new ScenarioExecutor(logger, guiExecutablePath, guiBounds);
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
