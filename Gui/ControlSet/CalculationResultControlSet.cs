using System;
using System.Windows.Forms;

namespace Gui.ControlSet
{
	internal sealed class CalculationResultControlSet
	{
		public TextBox MitamaOnlyTextBox { get; }
		public TextBox FinalStatsTextBox { get; }

		public CalculationResultControlSet(TextBox mitamaOnlyTextBox, TextBox finalStatsTextBox)
		{
			if (mitamaOnlyTextBox == null)
			{
				throw new ArgumentNullException(nameof(mitamaOnlyTextBox));
			}

			if (finalStatsTextBox == null)
			{
				throw new ArgumentNullException(nameof(finalStatsTextBox));
			}

			MitamaOnlyTextBox = mitamaOnlyTextBox;
			FinalStatsTextBox = finalStatsTextBox;
		}
	}
}
