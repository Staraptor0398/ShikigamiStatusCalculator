using Gui.Converter;
using SaveData.Access;
using SaveData.Model;

namespace Gui.Function
{
	internal enum SnapshotComparisonOutcome
	{
		SUCCESS,
		SNAPSHOT_NULL,
		STATUS_NULL
	}

	internal sealed class SnapshotComparisonFunction
	{
		public SnapshotComparisonOutcome TryCompare(string baseFilePath, string targetFilePath, out string baseSnapshotName, out string targetSnapshotName, out StatusComparisonResultDto comparisonResult)
		{
			baseSnapshotName = null;
			targetSnapshotName = null;
			comparisonResult = null;

			CalculationSnapshotSaveData baseSnapshot = SaveDataAccess.LoadSnapshot(baseFilePath);
			CalculationSnapshotSaveData targetSnapshot = SaveDataAccess.LoadSnapshot(targetFilePath);

			if (baseSnapshot == null || targetSnapshot == null)
			{
				return SnapshotComparisonOutcome.SNAPSHOT_NULL;
			}

			StatusDto baseStatus = StatusConverter.ToDto(baseSnapshot.FinalStatus);
			StatusDto targetStatus = StatusConverter.ToDto(targetSnapshot.FinalStatus);

			if (baseStatus == null || targetStatus == null)
			{
				return SnapshotComparisonOutcome.STATUS_NULL;
			}

			comparisonResult = ComparisonGateway.CompareStatus(baseStatus, targetStatus);

			baseSnapshotName = baseSnapshot.SnapshotName;
			targetSnapshotName = targetSnapshot.SnapshotName;

			return SnapshotComparisonOutcome.SUCCESS;
		}
	}
}
