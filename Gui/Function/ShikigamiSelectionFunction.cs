using Gui.Definition;
using Gui.Formatter;
using Gui.Logging;
using Gui.Operator;
using Gui.ShikigamiData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gui.Function
{
	internal sealed class ShikigamiSelectionFunction
	{
		private readonly ShikigamiSelectionOperator mOperator;
		private readonly PerformanceTracker mPerformanceTracker;
		private readonly Action<string> mMarkCalculationResultDirty;

		private List<ShikigamiDto> mShikigamiList = null;

		public ShikigamiSelectionFunction(ShikigamiSelectionOperator inputOperator, PerformanceTracker performanceTracker, Action<string> markCalculationResultDirty)
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

			mOperator = inputOperator;
			mPerformanceTracker = performanceTracker;
			mMarkCalculationResultDirty = markCalculationResultDirty;
		}

		public void InitializeRarityFilterComboBox(EventHandler handler)
		{
			mOperator.InitializeRarityFilterComboBox(handler);

			using (mPerformanceTracker.Track("RarityFilter.ApplyFilterExplicit"))
			{
				ApplyShikigamiFilter();
			}
		}

		public void InitializeShikigamiComboBox()
		{
			List<ShikigamiDto> shikigamiList;

			try
			{
				using (mPerformanceTracker.Track("Shikigami.LoadData"))
				{
					var outcome = ShikigamiGateway.GetShikigamiList(AppPath.ShikigamiDataCsvPath, out shikigamiList);

					if (ShikigamiDataErrorHandler.Handle(outcome, "式神データ読み込み"))
					{
						ShikigamiDataFileManager.MoveBrokenFile();
						ShikigamiDataFileManager.RestoreDefaultIfMissing();

						outcome = ShikigamiGateway.GetShikigamiList(AppPath.ShikigamiDataCsvPath, out shikigamiList);

						if (ShikigamiDataErrorHandler.Handle(outcome, "式神データ復元後読み込み"))
						{
							shikigamiList = new List<ShikigamiDto>();
						}
					}
				}
			}
			catch (Exception ex)
			{
				ShikigamiDataErrorHandler.Handle(ex, "式神データ読み込み");

				if (mShikigamiList == null)
				{
					mShikigamiList = new List<ShikigamiDto>();
				}

				return;
			}

			mShikigamiList = shikigamiList;

			using (mPerformanceTracker.Track("Shikigami.SetDisplayMember"))
			{
				mOperator.SetShikigamiDisplayMember("Name");
			}

			using (mPerformanceTracker.Track("Shikigami.SetItems"))
			{
				mOperator.SetShikigamiItems(getFilteredShikigamiList());
			}

			using (mPerformanceTracker.Track("Shikigami.ResetSelection"))
			{
				mOperator.ClearShikigamiSelection();
				mOperator.SetBaseStatsText("");
			}
		}

		public void HandleShikigamiSelectedIndexChanged()
		{
			using (mPerformanceTracker.Track("Shikigami.SelectedIndexChanged"))
			{
				ShikigamiDto selected = mOperator.GetSelectedShikigami();

				using (mPerformanceTracker.Track("Shikigami.FormatBaseSummary"))
				{
					mOperator.SetBaseStatsText(StatusFormatter.FormatBaseSummary(selected?.Status));
				}

				using (mPerformanceTracker.Track("Shikigami.MarkCalculationResultDirty"))
				{
					mMarkCalculationResultDirty("Input.Shikigami");
				}
			}
		}

		public void HandleRarityFilterSelectedIndexChanged()
		{
			using (mPerformanceTracker.Track("RarityFilter.SelectedIndexChanged"))
			{
				ApplyShikigamiFilter();
			}
		}

		public void ApplyShikigamiFilter()
		{
			using (mPerformanceTracker.Track("RarityFilter.ApplyFilter"))
			{
				var selectedShikigami = mOperator.GetSelectedShikigami();
				string selectedShikigamiName = selectedShikigami?.Name;

				using (mPerformanceTracker.Track("RarityFilter.SetShikigamiItems"))
				{
					mOperator.SetShikigamiItems(getFilteredShikigamiList());
				}

				mOperator.ClearShikigamiSelection();

				if (!string.IsNullOrEmpty(selectedShikigamiName))
				{
					mOperator.TrySelectShikigami(selectedShikigamiName);
				}
			}
		}

		private List<ShikigamiDto> getFilteredShikigamiList()
		{
			string selectedRarity = mOperator.GetRarityFilterText();

			if (mShikigamiList == null || selectedRarity == DisplayText.RARITY_ALL)
			{
				return mShikigamiList;
			}

			return mShikigamiList.Where(x => x.Rarity == selectedRarity).ToList();
		}

		public void ApplyShikigami(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return;
			}

			if (mOperator.TrySelectShikigami(name))
			{
				return;
			}

			mOperator.SetSelectedRarity(DisplayText.RARITY_ALL);

			if (mOperator.TrySelectShikigami(name))
			{
				return;
			}

			throw new InvalidOperationException($"Failed to select Shikigami: {name}");
		}

		public void ValidateBuildShikigami(string shikigamiName)
		{
			if (string.IsNullOrEmpty(shikigamiName))
			{
				return;
			}

			if (mShikigamiList == null || !mShikigamiList.Any(x => x != null && x.Name == shikigamiName))
			{
				throw new InvalidOperationException($"Shikigami not found: {shikigamiName}");
			}
		}
	}
}
