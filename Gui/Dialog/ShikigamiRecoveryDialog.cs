using System.Collections.Generic;
using System.Windows.Forms;

namespace Gui.Dialog
{
	public partial class ShikigamiRecoveryDialog : System.Windows.Forms.Form
	{
		private List<ShikigamiDto> mRecoveryCandidates;

		public List<ShikigamiDto> SelectedRecoveryCandidates = new List<ShikigamiDto>();

		public ShikigamiRecoveryDialog(List<ShikigamiDto> recoveryCandidates)
		{
			mRecoveryCandidates = recoveryCandidates;

			InitializeComponent();

			loadRecoveryCandidates();
			updateRecoveryCandidateCount();
		}

		private void loadRecoveryCandidates()
		{
			foreach (var shikigami in mRecoveryCandidates)
			{
				int rowIndex = dgvRecoveryCandidates.Rows.Add(
					true,
					shikigami.Rarity,
					shikigami.Name,
					shikigami.Status.Attack,
					shikigami.Status.HP,
					shikigami.Status.Defense,
					shikigami.Status.Speed
				);

				dgvRecoveryCandidates.Rows[rowIndex].Tag = shikigami;
			}
		}

		private void updateRecoveryCandidateCount()
		{
			lblRecoveryCandidateCount.Text = $"復旧候補：{mRecoveryCandidates.Count}件";
		}

		private void btnSelectAll_Click(object sender, System.EventArgs e)
		{
			foreach (DataGridViewRow row in dgvRecoveryCandidates.Rows)
			{
				row.Cells["columnRecovery"].Value = true;
			}
		}

		private void btnAllClear_Click(object sender, System.EventArgs e)
		{
			foreach (DataGridViewRow row in dgvRecoveryCandidates.Rows)
			{
				row.Cells["columnRecovery"].Value = false;
			}
		}

		private void btnRecovery_Click(object sender, System.EventArgs e)
		{
			SelectedRecoveryCandidates.Clear();

			foreach (DataGridViewRow row in dgvRecoveryCandidates.Rows)
			{
				bool isChecked = (bool)row.Cells["columnRecovery"].Value;

				if (!isChecked)
				{
					continue;
				}

				SelectedRecoveryCandidates.Add((ShikigamiDto)row.Tag);
			}

			if (SelectedRecoveryCandidates.Count == 0)
			{
				MessageBox.Show("復旧する式神を選択してください。");
				return;
			}

			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}
	}
}
