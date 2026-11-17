using Gui.ControlSet;
using Gui.Definition;
using Gui.Logging;
using System;
using System.Collections.Generic;

namespace Gui.Operator
{
	internal sealed class ShikigamiSelectionOperator
	{
		private readonly ShikigamiSelectionControlSet mControls;
		private readonly ComboBoxOperator mComboBoxOperator;
		private readonly PerformanceTracker mPerformanceTracker;

		public ShikigamiSelectionOperator(ShikigamiSelectionControlSet controls, ComboBoxOperator comboBoxOperator, PerformanceTracker performanceTracker)
		{
			if (controls == null)
			{
				throw new ArgumentNullException(nameof(controls));
			}

			if (comboBoxOperator == null)
			{
				throw new ArgumentNullException(nameof(comboBoxOperator));
			}

			if (performanceTracker == null)
			{
				throw new ArgumentNullException(nameof(performanceTracker));
			}

			mControls = controls;
			mComboBoxOperator = comboBoxOperator;
			mPerformanceTracker = performanceTracker;
		}

		public void InitializeRarityFilterComboBox(EventHandler handler)
		{
			if (handler == null)
			{
				throw new ArgumentNullException(nameof(handler));
			}

			using (mPerformanceTracker.Track("RarityFilter.SetItems"))
			{
				mComboBoxOperator.SetItems(
					mControls.RarityFilterComboBox,
					"RarityFilter",
					DisplayText.RARITY_ALL,
					DisplayText.RARITY_UR,
					DisplayText.RARITY_SP,
					DisplayText.RARITY_SSR,
					DisplayText.RARITY_SR);
			}

			mControls.RarityFilterComboBox.SelectedIndexChanged -= handler;

			try
			{
				using (mPerformanceTracker.Track("RarityFilter.SelectAllWithoutHandler"))
				{
					mControls.RarityFilterComboBox.SelectedIndex = 0;
				}
			}
			finally
			{
				mControls.RarityFilterComboBox.SelectedIndexChanged += handler;
			}
		}

		public void SetShikigamiDisplayMember(string displayMember)
		{
			mControls.ShikigamiComboBox.DisplayMember = displayMember;
		}

		public void SetShikigamiItems(List<ShikigamiDto> shikigamiList)
		{
			mControls.ShikigamiComboBox.BeginUpdate();

			try
			{
				mControls.ShikigamiComboBox.Items.Clear();

				if (shikigamiList != null)
				{
					mControls.ShikigamiComboBox.Items.AddRange(shikigamiList.ToArray());
				}
			}
			finally
			{
				mControls.ShikigamiComboBox.EndUpdate();
			}
		}

		public ShikigamiDto GetSelectedShikigami()
		{
			return mControls.ShikigamiComboBox.SelectedItem as ShikigamiDto;
		}

		public string GetShikigamiText()
		{
			return mControls.ShikigamiComboBox.Text;
		}

		public string GetRarityFilterText()
		{
			return mControls.RarityFilterComboBox.Text;
		}

		public object GetSelectedRarity()
		{
			return mControls.RarityFilterComboBox.SelectedItem;
		}

		public void SetSelectedRarity(object rarity)
		{
			mControls.RarityFilterComboBox.SelectedItem = rarity;
		}

		public void SetBaseStatsText(string text)
		{
			mControls.BaseStatsTextBox.Text = text;
		}

		public void ClearShikigamiSelection()
		{
			mControls.ShikigamiComboBox.SelectedIndex = -1;
		}

		public bool TrySelectShikigami(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return false;
			}

			foreach (var item in mControls.ShikigamiComboBox.Items)
			{
				var shikigami = item as ShikigamiDto;

				if (shikigami != null && shikigami.Name == name)
				{
					mControls.ShikigamiComboBox.SelectedItem = shikigami;
					return true;
				}
			}

			return false;
		}
	}
}
