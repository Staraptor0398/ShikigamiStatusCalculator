using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CoreTestDataConverter.Output
{
	public static class TestDataFileNameGenerator
	{
		private static readonly Regex TestDataFilePattern = new Regex(@"^T(\d{3})\.json$", RegexOptions.IgnoreCase);

		public static string GenerateNext(string directoryPath)
		{
			if (string.IsNullOrWhiteSpace(directoryPath))
			{
				throw new ArgumentException("Directory path must not be empty.", nameof(directoryPath));
			}

			if (!Directory.Exists(directoryPath))
			{
				throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");
			}
			var maxNumber = Directory.EnumerateFiles(directoryPath, "*.json").Select(Path.GetFileName).Select(GetTestNumber).DefaultIfEmpty(0).Max();

			return $"T{maxNumber + 1:D3}.json";
		}

		private static int? GetTestNumber(string fileName)
		{
			var match = TestDataFilePattern.Match(fileName);

			if (!match.Success)
			{
				return null;
			}

			return int.Parse(match.Groups[1].Value);
		}
	}
}
