using ScenarioRunner.Execution;
using System;
using System.IO;
using System.Threading;

namespace ScenarioRunner.Automation.Waiter
{
	public class ShikigamiDataWaiter
	{
		private const int DEFAULT_TIMEOUT_MS = 5000;
		private const int DEFAULT_INTERVAL_MS = 100;

		public void WaitForAutoRepair(ScenarioExecutonContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			if (string.IsNullOrWhiteSpace(context.ShikigamiDataFilePath))
			{
				throw new ArgumentException("Shikigami data file path is empty.", nameof(context.ShikigamiDataFilePath));
			}

			int elapsed = 0;

			while (elapsed < DEFAULT_TIMEOUT_MS)
			{
				string brokenPath = context.ShikigamiBrokenDataFilePath;

				bool brokenCreated = !string.IsNullOrWhiteSpace(brokenPath) && File.Exists(brokenPath);

				bool shikigamiDataRestored = isFileReady(context.ShikigamiDataFilePath);

				if (brokenCreated && shikigamiDataRestored)
				{
					return;
				}

				Thread.Sleep(DEFAULT_INTERVAL_MS);
				elapsed += DEFAULT_INTERVAL_MS;
			}

			throw new InvalidOperationException("Shikigami auto repair was not completed within the timeout.");
		}

		private bool isFileReady(string filePath)
		{
			if (!File.Exists(filePath))
			{
				return false;
			}

			try
			{
				using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					return stream.Length > 0;
				}
			}
			catch (IOException)
			{
				return false;
			}
		}
	}
}
