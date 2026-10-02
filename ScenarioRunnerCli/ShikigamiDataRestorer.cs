using ScenarioRunner.Startup;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace ScenarioRunnerCli
{
	public class ShikigamiDataRestorer
	{
		private const int MAX_RESTORE_ATTEMPTS = 3;
		private const int RETRY_INTERVAL_MS = 500;
		private const int GUI_STARTUP_TIMEOUT_MS = 5000;
		private const int GUI_EXIT_TIMEOUT_MS = 5000;

		private const string DATA_DIRECTORY_NAME = "Data";
		private const string SHIKIGAMI_DATA_FILE_NAME = "ShikigamiData.csv";

		public bool Restore(string guiExecutablePath)
		{
			string resolvedGuiExecutablePath = GuiExecutablePathResolver.Resolve(guiExecutablePath);
			string guiDirectoryPath = Path.GetDirectoryName(resolvedGuiExecutablePath);
			string shikigamiDataFilePath = Path.Combine(guiDirectoryPath, DATA_DIRECTORY_NAME, SHIKIGAMI_DATA_FILE_NAME);

			Console.WriteLine("[Batch] Restoring ShikigamiData.csv...");

			for (int attempt = 1; attempt <= MAX_RESTORE_ATTEMPTS; attempt++)
			{
				Console.WriteLine($"[Batch] Restore attempt {attempt}/{MAX_RESTORE_ATTEMPTS}...");

				try
				{
					restore(resolvedGuiExecutablePath, guiDirectoryPath, shikigamiDataFilePath);

					Console.WriteLine("[Batch] ShikigamiData.csv restored.");

					return true;
				}
				catch (Exception ex)
				{
					Console.WriteLine($"[Batch] Restore attempt {attempt} failed: {ex.Message}");

					if (attempt < MAX_RESTORE_ATTEMPTS)
					{
						Thread.Sleep(RETRY_INTERVAL_MS);
					}
				}
			}

			Console.WriteLine($"[Batch] ShikigamiData.csv restore failed after {MAX_RESTORE_ATTEMPTS} attempts.");

			return false;
		}

		private static void restore(string guiExecutablePath, string guiDirectoryPath, string shikigamiDataFilePath)
		{
			if (File.Exists(shikigamiDataFilePath))
			{
				File.Delete(shikigamiDataFilePath);
			}

			var startInfo = new ProcessStartInfo
			{
				FileName = guiExecutablePath,
				WorkingDirectory = guiDirectoryPath,
				UseShellExecute = false
			};

			Process process = Process.Start(startInfo);

			if (process == null)
			{
				throw new InvalidOperationException("Gui.exe could not be started.");
			}

			try
			{
				if (!process.WaitForInputIdle(GUI_STARTUP_TIMEOUT_MS))
				{
					throw new TimeoutException("Gui.exe startup timed out.");
				}

				if (!File.Exists(shikigamiDataFilePath))
				{
					throw new InvalidOperationException("ShikigamiData.csv was not restored.");
				}
			}
			finally
			{
				closeGui(process);
				process.Dispose();
			}
		}

		private static void closeGui(Process process)
		{
			if (process.HasExited)
			{
				return;
			}

			process.CloseMainWindow();

			if (process.WaitForExit(GUI_EXIT_TIMEOUT_MS))
			{
				return;
			}

			process.Kill();

			if (!process.WaitForExit(GUI_EXIT_TIMEOUT_MS))
			{
				throw new TimeoutException("Gui.exe could not be terminated.");
			}
		}
	}
}
