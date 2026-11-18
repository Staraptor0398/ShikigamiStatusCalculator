using Gui.Converter;
using Gui.Dialog;
using Gui.Logging;
using Gui.Model;
using Gui.Operator;
using SaveData.Access;
using SaveData.Model;
using System;

namespace Gui.Function
{
	internal sealed class SaveDataLoadFunction
	{
		private readonly MitamaInputOperator mMitamaInputOperator;
		private readonly ShikigamiSelectionFunction mShikigamiSelectionFunction;
		private readonly PerformanceTracker mPerformanceTracker;

		public SaveDataLoadFunction(MitamaInputOperator mitamaInputOperator, ShikigamiSelectionFunction shikigamiSelectionFunction, PerformanceTracker performanceTracker)
		{
			if (mitamaInputOperator == null)
			{
				throw new ArgumentNullException(nameof(mitamaInputOperator));
			}

			if (shikigamiSelectionFunction == null)
			{
				throw new ArgumentNullException(nameof(shikigamiSelectionFunction));
			}

			if (performanceTracker == null)
			{
				throw new ArgumentNullException(nameof(performanceTracker));
			}

			mMitamaInputOperator = mitamaInputOperator;
			mShikigamiSelectionFunction = shikigamiSelectionFunction;
			mPerformanceTracker = performanceTracker;
		}

		public MitamaSetInputModel Load(SaveDataLoadType loadType, string filePath, out string shikigamiName)
		{
			shikigamiName = null;

			MitamaSetInputModel inputModel;

			if (loadType == SaveDataLoadType.Build)
			{
				BuildSaveData data;

				using (mPerformanceTracker.Track("SaveData.LoadFile"))
				{
					data = SaveDataAccess.LoadBuild(filePath, mPerformanceTracker.Track);
				}

				if (data == null)
				{
					throw new InvalidOperationException("Build save data is null.");
				}

				using (mPerformanceTracker.Track("SaveData.Convert"))
				{
					inputModel = MitamaSetConverter.ToInputModel(data.MitamaSet);
				}

				if (inputModel == null)
				{
					throw new InvalidOperationException("MitamaSet input model is null.");
				}

				using (mPerformanceTracker.Track("SaveData.Validate"))
				{
					mShikigamiSelectionFunction.ValidateBuildShikigami(data.ShikigamiName);

					mMitamaInputOperator.ValidateInputModel(inputModel);
				}

				shikigamiName = data.ShikigamiName;
			}
			else if (loadType == SaveDataLoadType.MitamaSet)
			{
				MitamaSetSaveData data;

				using (mPerformanceTracker.Track("SaveData.LoadFile"))
				{
					data = SaveDataAccess.LoadMitamaSet(filePath, mPerformanceTracker.Track);
				}

				using (mPerformanceTracker.Track("SaveData.Convert"))
				{
					inputModel = MitamaSetConverter.ToInputModel(data);
				}

				if (inputModel == null)
				{
					throw new InvalidOperationException("MitamaSet input model is null.");
				}

				using (mPerformanceTracker.Track("SaveData.Validate"))
				{
					mMitamaInputOperator.ValidateInputModel(inputModel);
				}
			}
			else
			{
				throw new InvalidOperationException($"Unsupported SaveData load type: {loadType}");
			}

			return inputModel;
		}
	}
}
