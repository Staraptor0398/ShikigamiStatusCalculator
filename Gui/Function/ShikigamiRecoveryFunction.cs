using System;
using System.Collections.Generic;

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
			mCreateBackup();

			return ShikigamiGateway.RecoveryShikigami(mCurrentFilePath, selectedShikigamiList);
		}
	}
}
