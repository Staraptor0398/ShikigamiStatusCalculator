using System.Windows.Forms;

namespace Gui.Form
{
	public partial class StatusComparisonResultForm : System.Windows.Forms.Form
	{
		private readonly string mBaseSnapshotName;
		private readonly string mTargetSnapshotName;
		private readonly StatusComparisonResultDto mComparisonResult;


		public StatusComparisonResultForm(string baseSnapshotName, string targetSnapshotName, StatusComparisonResultDto comparisonResult)
		{
			InitializeComponent();

			mBaseSnapshotName = baseSnapshotName;
			mTargetSnapshotName = targetSnapshotName;
			mComparisonResult = comparisonResult;
		}

		private void StatusComparisonResultForm_Load(object sender, System.EventArgs e)
		{
			lblBaseSnapshotName.Text = mBaseSnapshotName;
			lblTargetSnapshotName.Text = mTargetSnapshotName;

			initializeComparisonResultGrid();
			displayComparisonResult();

			dgvComparisonResult.ClearSelection();
		}

		private void initializeComparisonResultGrid()
		{
			dgvComparisonResult.Columns.Clear();
			dgvComparisonResult.Rows.Clear();

			dgvComparisonResult.AllowUserToAddRows = false;
			dgvComparisonResult.AllowUserToDeleteRows = false;
			dgvComparisonResult.ReadOnly = true;
			dgvComparisonResult.RowHeadersVisible = false;
			dgvComparisonResult.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			dgvComparisonResult.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

			dgvComparisonResult.Columns.Add("StatusName", "ステータス");
			dgvComparisonResult.Columns.Add("Difference", "差分");
		}

		private void displayComparisonResult()
		{
			dgvComparisonResult.Rows.Clear();

			addComparisonResultRow("攻撃力", mComparisonResult.AttackDifference);
			addComparisonResultRow("HP", mComparisonResult.HpDifference);
			addComparisonResultRow("防御力", mComparisonResult.DefenseDifference);
			addComparisonResultRow("素早さ", mComparisonResult.SpeedDifference);
			addComparisonResultRow("会心率", mComparisonResult.CriticalRateDifference);
			addComparisonResultRow("会心DMG", mComparisonResult.CriticalDamageDifference);
			addComparisonResultRow("効果命中", mComparisonResult.EffectHitDifference);
			addComparisonResultRow("効果抵抗", mComparisonResult.EffectResistDifference);

		}

		private void addComparisonResultRow(string statusName, double difference)
		{
			dgvComparisonResult.Rows.Add(statusName, formatDifference(difference));
		}

		private string formatDifference(double difference)
		{
			if (difference > 0)
			{
				return "+" + difference.ToString();
			}

			return difference.ToString();
		}

		private void btnClose_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}
	}
}
