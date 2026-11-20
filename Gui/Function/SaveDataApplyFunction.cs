using Gui.Dialog;
using Gui.Logging;
using Gui.Model;
using Gui.Operator;
using System;

namespace Gui.Function
{
	internal enum SaveDataApplyCheckpoint
	{
		AfterShikigamiSelection,
		AfterApply,
		BeforeRollbackRestore
	}

	internal sealed class SaveDataApplyFunction
	{
		private readonly MitamaInputOperator mMitamaInputOperator;
		private readonly ShikigamiSelectionOperator mShikigamiSelectionOperator;
		private readonly ShikigamiSelectionFunction mShikigamiSelectionFunction;
		private readonly PerformanceTracker mPerformanceTracker;
		private readonly Action<SaveDataApplyCheckpoint> mCheckpoint;

		public SaveDataApplyFunction(MitamaInputOperator mitamaInputOperator, ShikigamiSelectionOperator shikigamiSelectionOperator, ShikigamiSelectionFunction shikigamiSelectionFunction, PerformanceTracker performanceTracker, Action<SaveDataApplyCheckpoint> checkpoint = null)
		{
			if (mitamaInputOperator == null)
			{
				throw new ArgumentNullException(nameof(mitamaInputOperator));
			}

			if (shikigamiSelectionOperator == null)
			{
				throw new ArgumentNullException(nameof(shikigamiSelectionOperator));
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
			mShikigamiSelectionOperator = shikigamiSelectionOperator;
			mShikigamiSelectionFunction = shikigamiSelectionFunction;
			mPerformanceTracker = performanceTracker;
			mCheckpoint = checkpoint;
		}

		public void Apply(SaveDataLoadType loadType, string shikigamiName, MitamaSetInputModel inputModel)
		{
			if (loadType == SaveDataLoadType.Build)
			{
				applyWithRollback(() => applyBuild(shikigamiName, inputModel));
			}
			else if (loadType == SaveDataLoadType.MitamaSet)
			{
				applyWithRollback(() => applyMitamaSet(inputModel));
			}
			else
			{
				throw new InvalidOperationException($"Unsupported SaveData load type: {loadType}");
			}
		}

		private void applyBuild(string shikigamiName, MitamaSetInputModel inputModel)
		{
			using (mPerformanceTracker.Track("SaveData.ApplyBuild"))
			{
				mShikigamiSelectionFunction.ApplyShikigami(shikigamiName);

				mCheckpoint?.Invoke(SaveDataApplyCheckpoint.AfterShikigamiSelection);

				mMitamaInputOperator.ApplyInputModel(inputModel);
			}
		}

		private void applyMitamaSet(MitamaSetInputModel inputModel)
		{
			using (mPerformanceTracker.Track("SaveData.ApplyMitamaSet"))
			{
				mMitamaInputOperator.ApplyInputModel(inputModel);
			}
		}

		private void applyWithRollback(Action apply)
		{
			using (mPerformanceTracker.Track("SaveData.ApplyWithRollback"))
			{
				MitamaSetInputModel previousInputModel;

				using (mPerformanceTracker.Track("SaveData.RollbackSnapshot"))
				{
					previousInputModel = mMitamaInputOperator.CreateInputModel();
				}

				string previousShikigamiName = mShikigamiSelectionOperator.GetSelectedShikigami()?.Name;
				object previousRarity = mShikigamiSelectionOperator.GetSelectedRarity();

				try
				{
					using (mPerformanceTracker.Track("SaveData.ApplyUI"))
					{
						apply();

						mCheckpoint?.Invoke(SaveDataApplyCheckpoint.AfterApply);
					}
				}
				catch (Exception applyEx)
				{
					Logger.Warning("SaveData.Rollback", "Started");

					try
					{
						using (mPerformanceTracker.Track("SaveData.RollbackRestore"))
						{
							mCheckpoint?.Invoke(SaveDataApplyCheckpoint.BeforeRollbackRestore);

							mShikigamiSelectionOperator.SetSelectedRarity(previousRarity);

							if (string.IsNullOrEmpty(previousShikigamiName))
							{
								mShikigamiSelectionOperator.ClearShikigamiSelection();
							}
							else if (!mShikigamiSelectionOperator.TrySelectShikigami(previousShikigamiName))
							{
								throw new InvalidOperationException($"Failed to restore Shikigami: {previousShikigamiName}");
							}

							mMitamaInputOperator.ApplyInputModel(previousInputModel);
						}
					}
					catch (Exception rollbackEx)
					{
						Logger.Error("SaveData.Rollback", "Failed", rollbackEx);

						throw new SaveDataRollbackException(applyEx, rollbackEx);
					}

					Logger.Info("SaveData.Rollback", "Completed");

					throw;
				}
			}
		}
	}
}
