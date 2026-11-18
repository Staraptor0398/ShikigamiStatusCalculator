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
			if (mCalculationResultFunction.CanSaveCalculationSnapshot())
			{
				return SaveDataSaveLevel.SNAPSHOT_AVAILABLE;
			}

			if (mShikigamiSelectionOperator.GetSelectedShikigami() != null)
			{
				return SaveDataSaveLevel.BUILD_AVAILABLE;
			}

			return SaveDataSaveLevel.MITAMA_SET_ONLY;
		}

		public void Save(SaveDataSaveType saveType, string filePath)
		{
			var inputModel = mMitamaInputOperator.CreateInputModel();

			if (saveType == SaveDataSaveType.Build)
			{
				BuildSaveData data = new BuildSaveData
				{
					MitamaSet = MitamaSetConverter.ToSaveData(inputModel),
					ShikigamiName = mShikigamiSelectionOperator.GetShikigamiText()
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
					ShikigamiName = mShikigamiSelectionOperator.GetShikigamiText(),
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
