using Gui.Formatter;
using Gui.Logging;
using Gui.Operator;
using System;

namespace Gui.Function
{
	internal sealed class CalculationResultFunction
	{
		private readonly CalculationResultOperator mOperator;
		private readonly ShikigamiSelectionOperator mShikigamiSelectionOperator;
		private readonly Func<bool> mIsDirtyChangeSuppressed;
		private readonly Action<string> mUpdateSaveButtonEnabled;

		private CalculationResultDto mLastCalculationResult = null;
		private bool mIsCalculationResultDirty = true;

		public CalculationResultDto LastCalculationResult => mLastCalculationResult;

		public CalculationResultFunction(			CalculationResultOperator resultOperator,			ShikigamiSelectionOperator shikigamiSelectionOperator,			Func<bool> isDirtyChangeSuppressed,			Action<string> updateSaveButtonEnabled)
		{
			if (resultOperator == null)
			{
				throw new ArgumentNullException(nameof(resultOperator));
			}

			if (shikigamiSelectionOperator == null)
			{
				throw new ArgumentNullException(nameof(shikigamiSelectionOperator));
			}

			if (isDirtyChangeSuppressed == null)
			{
				throw new ArgumentNullException(nameof(isDirtyChangeSuppressed));
			}

			if (updateSaveButtonEnabled == null)
			{
				throw new ArgumentNullException(nameof(updateSaveButtonEnabled));
			}

			mOperator = resultOperator;
			mShikigamiSelectionOperator = shikigamiSelectionOperator;
			mIsDirtyChangeSuppressed = isDirtyChangeSuppressed;
			mUpdateSaveButtonEnabled = updateSaveButtonEnabled;
		}

		public void SetResult(CalculationResultDto result)
		{
			mLastCalculationResult = result;
		}

		public void MarkClean()
		{
			mIsCalculationResultDirty = false;
		}

		public void MarkDirty(string source = "Other")
		{
			if (mIsDirtyChangeSuppressed())
			{
				return;
			}

			mUpdateSaveButtonEnabled(source);

			if (mIsCalculationResultDirty)
			{
				return;
			}

			mIsCalculationResultDirty = true;

			Logger.Info("Operation=ステータス計算結果状態変更 Message=入力内容が変更されたため、計算結果をDirty状態に変更しました。");
		}

		public bool CanSaveCalculationSnapshot()
		{
			return mLastCalculationResult != null && !mIsCalculationResultDirty;
		}

		public void ShowResult()
		{
			if (mLastCalculationResult == null)
			{
				return;
			}

			mOperator.SetMitamaOnlyText(				StatusFormatter.FormatMitamaSummary(mLastCalculationResult.MitamaOnlyStatus));

			if (mShikigamiSelectionOperator.GetSelectedShikigami() != null)
			{
				mOperator.SetFinalStatsText(					StatusFormatter.FormatFinalSummary(mLastCalculationResult.FinalStatus));
			}
			else
			{
				mOperator.SetFinalStatsText("");
			}
		}

		public void ClearTexts()
		{
			mOperator.Clear();
		}

		public void DeleteResult()
		{
			mLastCalculationResult = null;
		}
	}
}
