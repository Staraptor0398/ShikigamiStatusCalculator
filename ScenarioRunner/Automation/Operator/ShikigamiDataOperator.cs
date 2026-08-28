using ScenarioRunner.Execution;
using System;
using System.Collections.Generic;
using System.IO;

namespace ScenarioRunner.Automation.Operator
{
	public class ShikigamiDataOperator
	{
		private const string SHIKIGAMI_DATA_RELATIVE_PATH = @"Data\ShikigamiData.csv";
		private const string BROKEN_HEADER = "BROKEN_HEADER";

		private readonly ShikigamiDataFileOperator mFileOperator;
		private readonly ShikigamiOperator mShikigamiOperator;

		public ShikigamiDataOperator()
		{
			mFileOperator = new ShikigamiDataFileOperator();
			mShikigamiOperator = new ShikigamiOperator();
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

		public void RemoveShikigami(ScenarioExecutonContext context, string shikigamiName)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			if (string.IsNullOrWhiteSpace(shikigamiName))
			{
				throw new ArgumentException("Shikigami name is empty.", nameof(shikigamiName));
			}

			string filePath = getShikigamiDataPath(context);
			string[] lines = mFileOperator.ReadAllLines(filePath);

			if (lines.Length == 0)
			{
				throw new InvalidOperationException("ShikigamiData.csv is empty.");
			}

			var result = new List<string> { lines[0] };

			bool removed = false;

			for (int i = 1; i < lines.Length; i++)
			{
				string[] columns = lines[i].Split(',');

				if (columns.Length > 1 && columns[1] == shikigamiName)
				{
					removed = true;
					continue;
				}

				result.Add(lines[i]);
			}

			if (!removed)
			{
				throw new InvalidOperationException($"Shikigami was not found: {shikigamiName}");
			}

			mFileOperator.WriteAllLines(filePath, result.ToArray());
		}

		public void CreateBackup(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			mShikigamiOperator.SelectFirst(session);
			mShikigamiOperator.SaveSelectedShikigamiWithoutChanges(session);
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
