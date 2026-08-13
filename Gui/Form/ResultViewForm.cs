using Gui.Formatter;
using System;

namespace Gui.Form
{
	public partial class ResultViewForm : System.Windows.Forms.Form
	{
		private readonly CalculationResultDto mResult;

		public ResultViewForm(CalculationResultDto result)
		{
			InitializeComponent();

			mResult = result;

			showResult();
		}

		private void showResult()
		{
			if (mResult == null)
			{
				return;
			}

			txtMitamaStatus.Text = StatusFormatter.FormatMitamaDetail(mResult.MitamaOnlyStatus);
			txtFinalStatus.Text = StatusFormatter.FormatFinalDetail(mResult.FinalStatus);

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
