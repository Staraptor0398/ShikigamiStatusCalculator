using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace ScenarioRunnerCli
{
	public class ScenarioBatchRunner
	{
		public bool Run(string directoryPath, string guiExecutablePath)
		{
			if (string.IsNullOrWhiteSpace(directoryPath))
			{
				throw new ArgumentException("Scenario directory path is empty.", nameof(directoryPath));
			}

			string resolvedDirectoryPath = Path.GetFullPath(directoryPath);

			if (!Directory.Exists(resolvedDirectoryPath))
			{
				throw new DirectoryNotFoundException($"Scenario directory was not found: {resolvedDirectoryPath}");
			}

			string[] scenarioPaths = Directory.GetFiles(resolvedDirectoryPath, "*.scenario", SearchOption.AllDirectories);
			Array.Sort(scenarioPaths, StringComparer.OrdinalIgnoreCase);

			if (scenarioPaths.Length == 0)
			{
				throw new InvalidOperationException("No Scenario files were found.");
			}

			var stopwatch = Stopwatch.StartNew();
			var runner = new ScenarioCliRunner();

			int executedCount = 0;
			int passedCount = 0;
			var failedScenarioPaths = new List<string>();

			Console.WriteLine("========================================");
			Console.WriteLine("Scenario Batch");
			Console.WriteLine("========================================");
			Console.WriteLine($"Directory: {resolvedDirectoryPath}");
			Console.WriteLine($"Scenarios: {scenarioPaths.Length}");

			foreach (string scenarioPath in scenarioPaths)
			{
				string relativeScenarioPath = getRelativePath(resolvedDirectoryPath, scenarioPath);

				Console.WriteLine();
				Console.WriteLine($"[Batch] Running: {relativeScenarioPath}");

				executedCount++;

				if (runner.Run(scenarioPath, guiExecutablePath, true))
				{
					passedCount++;
					Console.WriteLine($"[Batch] PASS: {relativeScenarioPath}");
					continue;
				}

				failedScenarioPaths.Add(relativeScenarioPath);
				Console.WriteLine($"[Batch] FAIL: {relativeScenarioPath}");
			}

			stopwatch.Stop();

			int failedCount = failedScenarioPaths.Count;
			bool isSuccess = failedCount == 0;

			Console.WriteLine();
			Console.WriteLine("========================================");
			Console.WriteLine("Scenario Batch Result");
			Console.WriteLine("========================================");
			Console.WriteLine($"Total: {scenarioPaths.Length}");
			Console.WriteLine($"Executed: {executedCount}");
			Console.WriteLine($"Passed: {passedCount}");
			Console.WriteLine($"Failed: {failedCount}");

			if (failedScenarioPaths.Count > 0)
			{
				Console.WriteLine();
				Console.WriteLine("Failed Scenarios:");

				foreach (string failedScenarioPath in failedScenarioPaths)
				{
					Console.WriteLine($"  {failedScenarioPath}");
				}
			}

			Console.WriteLine($"Elapsed: {stopwatch.Elapsed.TotalSeconds:F2} sec");
			Console.WriteLine($"Batch Result: {(isSuccess ? "PASS" : "FAIL")}");

			return isSuccess;
		}

		private static string getRelativePath(string directoryPath, string filePath)
		{
			string relativePath = filePath.Substring(directoryPath.Length);

			return relativePath.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		}
	}
}
