using Gui.Logging;
using Gui.Operator;
using Gui.Resolver;
using System;

namespace Gui.Function
{
	internal sealed class MitamaInputFunction
	{
		private readonly MitamaInputOperator mOperator;
		private readonly PerformanceTracker mPerformanceTracker;
		private readonly Action<string> mMarkCalculationResultDirty;
		private readonly Func<bool> mIsLoading;

		public MitamaInputFunction(MitamaInputOperator inputOperator, PerformanceTracker performanceTracker, Action<string> markCalculationResultDirty, Func<bool> isLoading)
		{
			if (inputOperator == null)
			{
				throw new ArgumentNullException(nameof(inputOperator));
			}
			if (performanceTracker == null)
			{
				throw new ArgumentNullException(nameof(performanceTracker));
			}
			if (markCalculationResultDirty == null)
			{
				throw new ArgumentNullException(nameof(markCalculationResultDirty));
			}
			if (isLoading == null)
			{
				throw new ArgumentNullException(nameof(isLoading));
			}

			mOperator = inputOperator;
			mPerformanceTracker = performanceTracker;
			mMarkCalculationResultDirty = markCalculationResultDirty;
			mIsLoading = isLoading;
		}

		public void InitializeComboBoxes()
		{
			using (mPerformanceTracker.Track("MainForm.InitializeMainStatComboBoxes"))
			{
				mOperator.InitializeMainStatComboBoxes();
			}

			using (mPerformanceTracker.Track("MainForm.InitializeSubStatComboBoxes"))
			{
				mOperator.InitializeSubStatComboBoxes();
			}

			using (mPerformanceTracker.Track("MainForm.InitializeSetEffectComboBoxes"))
			{
				mOperator.InitializeSetEffectComboBoxes();
			}

			using (mPerformanceTracker.Track("MainForm.InitializeUniqueEffectComboBoxes"))
			{
				mOperator.InitializeUniqueEffectComboBoxes();
			}
		}

		public void RegisterInputChangedEvents()
		{
			mOperator.RegisterInputChangedEvents(inputChanged);
		}

		public void UpdateMainStatValue(int slot)
		{
			using (mPerformanceTracker.Track("MainStat.UpdateValue"))
			using (mPerformanceTracker.Track(mIsLoading() ? "MainStat.UpdateValue.Loading" : "MainStat.UpdateValue.Other"))
			{
				string statText;

				using (mPerformanceTracker.Track("MainStat.GetSelectedItem"))
				{
					statText = mOperator.GetSelectedMainStatText(slot);
				}

				bool hasValue = !string.IsNullOrEmpty(statText);
				double value = 0;

				if (hasValue)
				{
					using (mPerformanceTracker.Track("MainStat.ResolveValue"))
					{
						value = MainStatValueResolver.Resolve(statText, slot);
					}
				}

				using (mPerformanceTracker.Track("MainStat.SetValueText"))
				{
					mOperator.SetMainStatValueText(slot, hasValue ? value.ToString() : "");
				}

				using (mPerformanceTracker.Track("MainStat.MarkDirty"))
				{
					mMarkCalculationResultDirty("Input.MainStat");
				}
			}
		}

		public void Clear()
		{
			mOperator.Clear();
		}

		private void inputChanged(object sender, EventArgs e)
		{
			mMarkCalculationResultDirty(mOperator.GetInputChangeSource(sender));
		}
	}
}
