using Gui.Logging;
using System;
using System.Collections.Generic;
using System.IO;

namespace Gui.Function
{
	internal sealed class ShikigamiRecoveryFunction
	{
		private readonly string mCurrentFilePath;
		private readonly Action mCreateBackup;

		public ShikigamiRecoveryFunction(string currentFilePath, Action createBackup)
		{
			if (string.IsNullOrWhiteSpace(currentFilePath))
			{
				throw new ArgumentException("Current Shikigami data file path is empty.", nameof(currentFilePath));
			}

			if (createBackup == null)
			{
				throw new ArgumentNullException(nameof(createBackup));
			}

			mCurrentFilePath = currentFilePath;
			mCreateBackup = createBackup;
		}

		public ShikigamiDataOutcomeDto GetRecoveryCandidates(string sourceFilePath, out List<ShikigamiDto> recoveryCandidates)
		{
			return ShikigamiGateway.GetRecoveryCandidateShikigamiList(mCurrentFilePath, sourceFilePath, out recoveryCandidates);
		}

		public ShikigamiDataOutcomeDto Recover(List<ShikigamiDto> selectedShikigamiList)
		{
			if (!File.Exists(mCurrentFilePath))
			{
				throw new FileNotFoundException("Current Shikigami data file was not found.", mCurrentFilePath);
			}

			mCreateBackup();

			Logger.Info("Shikigami.Recovery", "BackupCompleted");

			return ShikigamiGateway.RecoveryShikigami(mCurrentFilePath, selectedShikigamiList);
		}
	}
}
