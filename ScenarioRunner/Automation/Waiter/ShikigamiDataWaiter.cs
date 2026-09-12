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
		private const int REQUIRED_STABLE_COUNT = 3;

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
			int stableCount = 0;
			long previousLength = -1;
			DateTime previousLastWriteTimeUtc = DateTime.MinValue;

			while (elapsed < DEFAULT_TIMEOUT_MS)
			{
				if (isAutoRepairCompleted(context, ref stableCount, ref previousLength, ref previousLastWriteTimeUtc))
				{
					return;
				}

				Thread.Sleep(DEFAULT_INTERVAL_MS);
				elapsed += DEFAULT_INTERVAL_MS;
			}

			throw new InvalidOperationException("Shikigami auto repair was not completed within the timeout.");
		}

		private bool isAutoRepairCompleted(ScenarioExecutonContext context, ref int stableCount, ref long previousLength, ref DateTime previousLastWriteTimeUtc)
		{
			string brokenPath = context.ShikigamiBrokenDataFilePath;

			if (string.IsNullOrWhiteSpace(brokenPath) || !File.Exists(brokenPath))
			{
				resetFileState(ref stableCount, ref previousLength, ref previousLastWriteTimeUtc);
				return false;
			}

			return isFileStable(context.ShikigamiDataFilePath, ref stableCount, ref previousLength, ref previousLastWriteTimeUtc);
		}

		private bool isFileStable(string filePath, ref int stableCount, ref long previousLength, ref DateTime previousLastWriteTimeUtc)
		{
			if (!tryGetFileState(filePath, out long length, out DateTime lastWriteTimeUtc))
			{
				resetFileState(ref stableCount, ref previousLength, ref previousLastWriteTimeUtc);
				return false;
			}

			if (length != previousLength || lastWriteTimeUtc != previousLastWriteTimeUtc)
			{
				stableCount = 0;
				previousLength = length;
				previousLastWriteTimeUtc = lastWriteTimeUtc;
				return false;
			}

			stableCount++;

			return stableCount >= REQUIRED_STABLE_COUNT;
		}

		private bool tryGetFileState(string filePath, out long length, out DateTime lastWriteTimeUtc)
		{
			length = 0;
			lastWriteTimeUtc = DateTime.MinValue;

			if (!File.Exists(filePath))
			{
				return false;
			}

			try
			{
				using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
				{
					length = stream.Length;
				}

				if (length <= 0)
				{
					return false;
				}

				lastWriteTimeUtc = File.GetLastWriteTimeUtc(filePath);

				return true;
			}
			catch (IOException)
			{
				return false;
			}
		}

		private void resetFileState(ref int stableCount, ref long previousLength, ref DateTime previousLastWriteTimeUtc)
		{
			stableCount = 0;
			previousLength = -1;
			previousLastWriteTimeUtc = DateTime.MinValue;
		}
	}
}
