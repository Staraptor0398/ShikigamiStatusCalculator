using System;
using System.IO;

namespace ScenarioRunner.Automation.Operator
{
	public class ShikigamiDataFileOperator
	{
		public string[] ReadAllLines(string filePath)
		{
			if (string.IsNullOrWhiteSpace(filePath))
			{
				throw new ArgumentException("File path is empty.", nameof(filePath));
			}

			if (!File.Exists(filePath))
			{
				throw new FileNotFoundException("ShikigamiData.csv was not found.", filePath);
			}

			return File.ReadAllLines(filePath);
		}

		public void WriteAllLines(string filePath, string[] lines)
		{
			if (string.IsNullOrWhiteSpace(filePath))
			{
				throw new ArgumentException("File path is empty.", nameof(filePath));
			}

			if (lines == null)
			{
				throw new ArgumentNullException(nameof(lines));
			}

			File.WriteAllLines(filePath, lines);
		}
	}
}
