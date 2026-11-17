using Gui.Logging;
using Gui.Model;
using Gui.Operator;
using Gui.Validation;
using System;

namespace Gui.Function
{
	internal sealed class SaveDataButtonFunction
	{
		private readonly SaveDataButtonOperator mOperator;
		private readonly MitamaInputOperator mMitamaInputOperator;
		private readonly PerformanceTracker mPerformanceTracker;
		private readonly Func<bool> mIsIdle;

		public SaveDataButtonFunction(SaveDataButtonOperator buttonOperator, MitamaInputOperator mitamaInputOperator, PerformanceTracker performanceTracker, Func<bool> isIdle)
		{
			if (buttonOperator == null)
			{
				throw new ArgumentNullException(nameof(buttonOperator));
			}

			if (mitamaInputOperator == null)
			{
				throw new ArgumentNullException(nameof(mitamaInputOperator));
			}

			if (performanceTracker == null)
			{
				throw new ArgumentNullException(nameof(performanceTracker));
			}

			if (isIdle == null)
			{
				throw new ArgumentNullException(nameof(isIdle));
			}

			mOperator = buttonOperator;
			mMitamaInputOperator = mitamaInputOperator;
			mPerformanceTracker = performanceTracker;
			mIsIdle = isIdle;
		}

		public void Update(string source = "Other")
		{
			using (mPerformanceTracker.Track("MainForm.UpdateSaveButtonEnabled"))
			using (mPerformanceTracker.Track($"SaveButton.Update.Source.{source}"))
			{
				bool isIdle = mIsIdle();

				using (mPerformanceTracker.Track("SaveButton.SetLoadEnabled"))
				{
					mOperator.SetLoadEnabled(isIdle);
				}

				if (!isIdle)
				{
					using (mPerformanceTracker.Track("SaveButton.SetSaveEnabled"))
					{
						mOperator.SetSaveEnabled(false);
					}

					return;
				}

				MitamaSetInputModel inputModel;

				using (mPerformanceTracker.Track("SaveButton.CreateInputModel"))
				{
					inputModel = mMitamaInputOperator.CreateInputModel();
				}

				CalculationInputValidationOutcome validationOutcome;

				using (mPerformanceTracker.Track("SaveButton.Validate"))
				{
					validationOutcome = CalculationInputValidator.Validate(inputModel);
				}

				using (mPerformanceTracker.Track("SaveButton.SetSaveEnabled"))
				{
					mOperator.SetSaveEnabled(validationOutcome != CalculationInputValidationOutcome.NO_EQUIPPED_MITAMA);
				}
			}
		}
	}
}
