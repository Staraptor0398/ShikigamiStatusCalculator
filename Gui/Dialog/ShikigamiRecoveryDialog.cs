using System.Collections.Generic;
using System.Windows.Forms;

namespace Gui.Dialog
{
	public partial class ShikigamiRecoveryDialog : System.Windows.Forms.Form
	{
		private List<ShikigamiDto> _recoveryCandinates;

		public List<ShikigamiDto> _selectedRecoveryCandinate = new List<ShikigamiDto>();

		public ShikigamiRecoveryDialog(List<ShikigamiDto> recoveryCandinates)
		{
			_recoveryCandinates = recoveryCandinates;

			InitializeComponent();

			loadRecoveryCandinates();
			updateRecoveryCandinateCount();
		}

		private void loadRecoveryCandinates()
		{
			foreach (var shikigami in _recoveryCandinates)
			{
				int rowIndex = dgvRecoveryCandinates.Rows.Add(
					true,
					shikigami.Rarity,
					shikigami.Name,
					shikigami.Attack,
					shikigami.HP,
					shikigami.Defense,
					shikigami.Speed
				);

				dgvRecoveryCandinates.Rows[rowIndex].Tag = shikigami;
			}
		}

		private void updateRecoveryCandinateCount()
		{
			lblRecoveryCandinateCount.Text = $"復旧候補：{_recoveryCandinates.Count}件";
		}

		private void btnSelectAll_Click(object sender, System.EventArgs e)
		{
			foreach (DataGridViewRow row in dgvRecoveryCandinates.Rows)
			{
				row.Cells["columnRecovery"].Value = true;
			}
		}

		private void btnAllClear_Click(object sender, System.EventArgs e)
		{
			foreach (DataGridViewRow row in dgvRecoveryCandinates.Rows)
			{
				row.Cells["columnRecovery"].Value = false;
			}
		}

		private void btnRecovery_Click(object sender, System.EventArgs e)
		{
			_selectedRecoveryCandinate.Clear();

			foreach (DataGridViewRow row in dgvRecoveryCandinates.Rows)
			{
				bool isCecked = (bool)row.Cells["columnRecovery"].Value;

				if (!isCecked)
				{
					continue;
				}

				_selectedRecoveryCandinate.Add((ShikigamiDto)row.Tag);
			}

			if (_selectedRecoveryCandinate.Count == 0)
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
