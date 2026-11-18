using Gui.Converter;
using Gui.Logging;
using Gui.Model;
using Gui.Operator;
using Gui.Validation;
using System;

#if DEBUG
using SaveData.Model.Development;
#endif

namespace Gui.Function
{
	internal sealed class CalculationFunction
	{
		private readonly MitamaInputOperator mMitamaInputOperator;
		private readonly ShikigamiSelectionOperator mShikigamiSelectionOperator;
		private readonly PerformanceTracker mPerformanceTracker;
		private readonly Func<CalculationInputValidationOutcome, bool> mHandleValidationError;

		public CalculationFunction(MitamaInputOperator mitamaInputOperator, ShikigamiSelectionOperator shikigamiSelectionOperator, PerformanceTracker performanceTracker, Func<CalculationInputValidationOutcome, bool> handleValidationError)
		{
			if (mitamaInputOperator == null)
			{
				throw new ArgumentNullException(nameof(mitamaInputOperator));
			}

			if (shikigamiSelectionOperator == null)
			{
				throw new ArgumentNullException(nameof(shikigamiSelectionOperator));
			}

			if (performanceTracker == null)
			{
				throw new ArgumentNullException(nameof(performanceTracker));
			}

			if (handleValidationError == null)
			{
				throw new ArgumentNullException(nameof(handleValidationError));
			}

			mMitamaInputOperator = mitamaInputOperator;
			mShikigamiSelectionOperator = shikigamiSelectionOperator;
			mPerformanceTracker = performanceTracker;
			mHandleValidationError = handleValidationError;
		}

#if DEBUG
		public bool TryExecute(out CalculationResultDto calculationResult, out CalculationTestSource calculationTestSource)
#else
		public bool TryExecute(out CalculationResultDto calculationResult)
#endif
		{
			calculationResult = null;

#if DEBUG
			calculationTestSource = null;
#endif

			MitamaSetInputModel inputModel;

			using (mPerformanceTracker.Track("Calculation.CreateInputModel"))
			{
				inputModel = mMitamaInputOperator.CreateInputModel();
			}

			CalculationInputValidationOutcome validationOutcome;

			using (mPerformanceTracker.Track("Calculation.Validate"))
			{
				validationOutcome = CalculationInputValidator.Validate(inputModel);
			}

			if (mHandleValidationError(validationOutcome))
			{
				return false;
			}

			StatusDto baseStatus;
			MitamaSetDto mitamaSet;

			using (mPerformanceTracker.Track("Calculation.ConvertInput"))
			{
				var selectedShikigami = mShikigamiSelectionOperator.GetSelectedShikigami();

				baseStatus = selectedShikigami?.Status ?? new StatusDto();
				mitamaSet = MitamaSetConverter.ToDto(inputModel);
			}

			using (mPerformanceTracker.Track("Calculation.Gateway"))
			{
				calculationResult = CalculationGateway.Calculate(baseStatus, mitamaSet);
			}

#if DEBUG
			calculationTestSource = CalculationTestSourceConverter.ToSaveData(baseStatus, mitamaSet, calculationResult);
#endif

			return true;
		}
	}
}
