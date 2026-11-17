using System;
using System.Windows.Forms;

namespace Gui.ControlSet
{
	internal sealed class ShikigamiSelectionControlSet
	{
		public ComboBox ShikigamiComboBox { get; }
		public ComboBox RarityFilterComboBox { get; }
		public TextBox BaseStatsTextBox { get; }

		public ShikigamiSelectionControlSet(ComboBox shikigamiComboBox, ComboBox rarityFilterComboBox, TextBox baseStatsTextBox)
		{
			if (shikigamiComboBox == null)
			{
				throw new ArgumentNullException(nameof(shikigamiComboBox));
			}

			if (rarityFilterComboBox == null)
			{
				throw new ArgumentNullException(nameof(rarityFilterComboBox));
			}

			if (baseStatsTextBox == null)
			{
				throw new ArgumentNullException(nameof(baseStatsTextBox));
			}

			ShikigamiComboBox = shikigamiComboBox;
			RarityFilterComboBox = rarityFilterComboBox;
			BaseStatsTextBox = baseStatsTextBox;
		}
	}
}
