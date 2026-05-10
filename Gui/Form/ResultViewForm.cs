using Gui.Common;
using System;
using System.Windows.Forms;

namespace ShikigamiApp
{
	public partial class ResultViewForm : Form
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

			txtMitamaStatus.Text = formatMitamaStatus(_result.MitamaOnlyStatus);
			txtFinalStatus.Text = formatFinalStatus(_result.FinalStatus);

			txtMitamaStatus.SelectionLength = 0;
			txtFinalStatus.SelectionLength = 0;

			btnClose.Focus();
		}

		private string formatMitamaStatus(StatusDto s)
		{
			if (s == null)
			{
				return "";
			}

			return
					$"{DisplayText.Attack,-8}: {s.Attack:F2}\r\n" +
					$"{DisplayText.HP,-11}: {s.HP:F2}\r\n" +
					$"{DisplayText.Deffense,-8}: {s.Defense:F2}\r\n" +
					$"{DisplayText.Speed,-8}: {s.Speed:F2}\r\n" +

					$"{DisplayText.AdditionalAttack,-6}: {s.AdditionalAttackRate:F2}%\r\n" +
					$"{DisplayText.AdditionalHP,-9}: {s.AdditionalHpRate:F2}%\r\n" +
					$"{DisplayText.AdditionalDeffense,-6}: {s.AdditionalDeffenseRate:F2}%\r\n" +

					$"{DisplayText.CritRate,-8}: {s.CritRate:F2}%\r\n" +
					$"{DisplayText.CritDamage,-9}: {s.CritDamage:F2}%\r\n" +
					$"{DisplayText.EffectHit,-7}: {s.EffectHit:F2}%\r\n" +
					$"{DisplayText.EffectResist,-7}: {s.EffectResist:F2}%";

		}

		private string formatFinalStatus(StatusDto s)
		{
			if (s == null)
			{
				return "";
			}

			return
					$"{DisplayText.Attack,-8}: {s.Attack:F2}\r\n" +
					$"{DisplayText.HP,-11}: {s.HP:F2}\r\n" +
					$"{DisplayText.Deffense,-8}: {s.Defense:F2}\r\n" +
					$"{DisplayText.Speed,-8}: {s.Speed:F2}\r\n" +
					$"{DisplayText.CritRate,-8}: {s.CritRate:F2}%\r\n" +
					$"{DisplayText.CritDamage,-9}: {s.CritDamage:F2}%\r\n" +
					$"{DisplayText.EffectHit,-7}: {s.EffectHit:F2}%\r\n" +
					$"{DisplayText.EffectResist,-7}: {s.EffectResist:F2}%";
		}

		private void btnClose_Click(object sender, EventArgs e)
		{
			this.Close();
		}
	}
}
