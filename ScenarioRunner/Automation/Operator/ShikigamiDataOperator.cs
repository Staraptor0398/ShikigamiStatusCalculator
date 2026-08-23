using ScenarioRunner.Execution;
using System;
using System.IO;

namespace ScenarioRunner.Automation.Operator
{
	public class ShikigamiDataOperator
	{
		private const string SHIKIGAMI_DATA_RELATIVE_PATH = @"Data\ShikigamiData.csv";
		private const string BROKEN_HEADER = "BROKEN_HEADER";

		private readonly ShikigamiDataFileOperator mFileOperator;

		public ShikigamiDataOperator()
		{
			mFileOperator = new ShikigamiDataFileOperator();
		}

		public void BreakHeader(ScenarioExecutonContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			string filePath = getShikigamiDataPath(context);
			string[] lines = mFileOperator.ReadAllLines(filePath);

			if (lines.Length == 0)
			{
				throw new InvalidOperationException("ShikigamiData.csv is empty.");
			}

			lines[0] = BROKEN_HEADER;

			mFileOperator.WriteAllLines(filePath, lines);
		}

		private string getShikigamiDataPath(ScenarioExecutonContext context)
		{
			string guiDirectoryPath = Path.GetDirectoryName(context.GuiExecutablePath);

			if (string.IsNullOrWhiteSpace(guiDirectoryPath))
			{
				throw new InvalidOperationException("Gui executable directory could not be resolved.");
			}

			return Path.Combine(guiDirectoryPath, SHIKIGAMI_DATA_RELATIVE_PATH);
		}
	}
}
