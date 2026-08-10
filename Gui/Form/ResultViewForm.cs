using Gui.Formatter;
using System;

namespace Gui.Form
{
	public partial class ResultViewForm : System.Windows.Forms.Form
	{
		private readonly CalculationResultDto _result;

		public ResultViewForm(CalculationResultDto result)
		{
			InitializeComponent();

			_result = result;

			showResult();
		}

		private void showResult()
		{
			if (_result == null)
			{
				return;
			}

			txtMitamaStatus.Text = StatusFormatter.FormatMitamaDetail(_result.MitamaOnlyStatus);
			txtFinalStatus.Text = StatusFormatter.FormatFinalDetail(_result.FinalStatus);

			txtMitamaStatus.SelectionLength = 0;
			txtFinalStatus.SelectionLength = 0;

			btnClose.Focus();
		}

		private void btnClose_Click(object sender, EventArgs e)
		{
			this.Close();
		}
	}
}
