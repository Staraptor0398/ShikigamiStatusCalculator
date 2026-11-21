using DiagnosticLogAnalyzer.Model;
using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text.RegularExpressions;

namespace DiagnosticLogAnalyzer.Reader
{
	public static class AnalysisInputReader
	{
		public static AnalysisInput Read(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				throw new ArgumentException("Input JSON path is required.");
			}

			string fullPath = Path.GetFullPath(path);
			AnalysisInput input;
			DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(AnalysisInput));

			using (FileStream stream = File.OpenRead(fullPath))
			{
				input = (AnalysisInput)serializer.ReadObject(stream);
			}

			if (input == null || input.SchemaVersion != 1)
			{
				throw new InvalidDataException("Unsupported DiagnosticAnalysisInput schemaVersion.");
			}
			if (string.IsNullOrWhiteSpace(input.RunId) || !Regex.IsMatch(input.RunId, @"^\d+$"))
			{
				throw new InvalidDataException("Invalid RunId.");
			}
			if (input.DiagnosticLogs == null || input.Scenarios == null || input.PerformanceSessions == null || input.PreparationIssues == null)
			{
				throw new InvalidDataException("Required input arrays are missing or null.");
			}

			string root = Path.GetDirectoryName(fullPath);
			foreach (DiagnosticLogFile log in input.DiagnosticLogs)
			{
				if (log == null)
				{
					throw new InvalidDataException("Null diagnostic log entry.");
				}

				ResolveSourcePath(root, log.Path);
			}
			foreach (ScenarioRecord scenario in input.Scenarios)
			{
				if (scenario == null || string.IsNullOrEmpty(scenario.Scenario) || scenario.Commands == null || scenario.GuiWindows == null)
				{
					throw new InvalidDataException("Invalid scenario entry.");
				}

				ResolveSourcePath(root, scenario.SourceFile);
			}
			foreach (PerformanceSessionRecord session in input.PerformanceSessions)
			{
				if (session == null || string.IsNullOrWhiteSpace(session.SessionId))
				{
					throw new InvalidDataException("Invalid Performance Session entry.");
				}
				if (!string.IsNullOrEmpty(session.ScenarioFile))
				{
					ResolveSourcePath(root, session.ScenarioFile);
				}
			}

			return input;
		}

		public static string ResolveSourcePath(string runDirectory, string relativePath)
		{
			if (string.IsNullOrWhiteSpace(relativePath) || Path.IsPathRooted(relativePath) || relativePath.IndexOf('\\') >= 0 || relativePath.Contains(":"))
			{
				throw new InvalidDataException("Expected a portable relative path: " + relativePath);
			}

			string root = Path.GetFullPath(runDirectory).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			string fullPath = Path.GetFullPath(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));
			string prefix = root + Path.DirectorySeparatorChar;

			if (!fullPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidDataException("Source path escapes RunDirectory: " + relativePath);
			}

			return fullPath;
		}
	}
}
