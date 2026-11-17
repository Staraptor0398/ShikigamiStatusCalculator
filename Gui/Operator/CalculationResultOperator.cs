using Gui.ControlSet;
using System;

namespace Gui.Operator
{
	internal sealed class CalculationResultOperator
	{
		private readonly CalculationResultControlSet mControls;

		public CalculationResultOperator(CalculationResultControlSet controls)
		{
			if (controls == null)
			{
				throw new ArgumentNullException(nameof(controls));
			}

			mControls = controls;
		}

		public void SetMitamaOnlyText(string text)
		{
			mControls.MitamaOnlyTextBox.Text = text;
		}

		public void SetFinalStatsText(string text)
		{
			mControls.FinalStatsTextBox.Text = text;
		}

		public void Clear()
		{
			mControls.MitamaOnlyTextBox.Text = "";
			mControls.FinalStatsTextBox.Text = "";
		}
	}
}
