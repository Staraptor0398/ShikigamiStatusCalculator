using Gui.Converter;
using Gui.Definition;
using Gui.Dialog;
using Gui.Operator;
using SaveData.Access;
using SaveData.Model;
using System;
using System.IO;

namespace Gui.Function
{
	internal sealed class SaveDataSaveFunction
	{
		private readonly MitamaInputOperator mMitamaInputOperator;
		private readonly ShikigamiSelectionOperator mShikigamiSelectionOperator;
		private readonly CalculationResultFunction mCalculationResultFunction;

		public SaveDataSaveFunction(MitamaInputOperator mitamaInputOperator, ShikigamiSelectionOperator shikigamiSelectionOperator, CalculationResultFunction calculationResultFunction)
		{
			if (mitamaInputOperator == null)
			{
				throw new ArgumentNullException(nameof(mitamaInputOperator));
			}

			if (shikigamiSelectionOperator == null)
			{
				throw new ArgumentNullException(nameof(shikigamiSelectionOperator));
			}

			if (calculationResultFunction == null)
			{
				throw new ArgumentNullException(nameof(calculationResultFunction));
			}

			mMitamaInputOperator = mitamaInputOperator;
			mShikigamiSelectionOperator = shikigamiSelectionOperator;
			mCalculationResultFunction = calculationResultFunction;
		}

		public SaveDataSaveLevel GetSaveLevel()
		{
			if (mShikigamiSelectionOperator.GetSelectedShikigami() == null)
			{
				return SaveDataSaveLevel.MITAMA_SET_ONLY;
			}

			if (mCalculationResultFunction.CanSaveCalculationSnapshot())
			{
				return SaveDataSaveLevel.SNAPSHOT_AVAILABLE;
			}

			return SaveDataSaveLevel.BUILD_AVAILABLE;
		}

		public void Save(SaveDataSaveType saveType, string filePath)
		{
			if (string.IsNullOrWhiteSpace(filePath))
			{
				throw new ArgumentException("SaveData file path is empty.", nameof(filePath));
			}

			if (saveType != SaveDataSaveType.Build && saveType != SaveDataSaveType.MitamaSet && saveType != SaveDataSaveType.CalculationSnapshot)
			{
				throw new InvalidOperationException($"Unsupported SaveData save type: {saveType}");
			}

			var selectedShikigami = mShikigamiSelectionOperator.GetSelectedShikigami();

			if (saveType != SaveDataSaveType.MitamaSet && selectedShikigami == null)
			{
				throw new InvalidOperationException("Shikigami is not selected.");
			}

			if (saveType == SaveDataSaveType.CalculationSnapshot && !mCalculationResultFunction.CanSaveCalculationSnapshot())
			{
				throw new InvalidOperationException("Calculation snapshot is not available.");
			}

			var inputModel = mMitamaInputOperator.CreateInputModel();

			if (saveType == SaveDataSaveType.Build)
			{
				BuildSaveData data = new BuildSaveData
				{
					MitamaSet = MitamaSetConverter.ToSaveData(inputModel),
					ShikigamiName = selectedShikigami.Name
				};

				SaveDataAccess.SaveBuild(filePath, data);
			}
			else if (saveType == SaveDataSaveType.MitamaSet)
			{
				var data = MitamaSetConverter.ToSaveData(inputModel);

				SaveDataAccess.SaveMitamaSet(filePath, data);
			}
			else if (saveType == SaveDataSaveType.CalculationSnapshot)
			{
				CalculationResultDto calculationResult = mCalculationResultFunction.LastCalculationResult;

				CalculationSnapshotSaveData data = new CalculationSnapshotSaveData
				{
					SnapshotName = createSnapshotNameFromFilePath(filePath),
					CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
					ShikigamiName = selectedShikigami.Name,
					MitamaSet = MitamaSetConverter.ToSaveData(inputModel),
					MitamaStatus = StatusConverter.ToSaveData(calculationResult.MitamaOnlyStatus),
					FinalStatus = StatusConverter.ToSaveData(calculationResult.FinalStatus)
				};

				SaveDataAccess.SaveSnapshot(filePath, data);
			}
		}

		private string createSnapshotNameFromFilePath(string filePath)
		{
			string fileName = Path.GetFileName(filePath);

			if (fileName.EndsWith(SaveDataFileDefinition.SNAPSHOT_EXTENSION))
			{
				return fileName.Substring(0, fileName.Length - SaveDataFileDefinition.SNAPSHOT_EXTENSION.Length);
			}

			return Path.GetFileNameWithoutExtension(filePath);
		}
	}
}
