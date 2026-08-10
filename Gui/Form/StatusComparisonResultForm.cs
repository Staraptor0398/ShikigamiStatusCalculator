using System.Windows.Forms;

namespace Gui.Form
{
	public partial class StatusComparisonResultForm : System.Windows.Forms.Form
	{
		private readonly string _baseSnapshotName;
		private readonly string _targetSnapshotName;
		private readonly StatusComparisonResultDto _comparisonResult;


		public StatusComparisonResultForm(string baseSnapshotName, string targetSnapshotName, StatusComparisonResultDto comparisonResult)
		{
			InitializeComponent();

			_baseSnapshotName = baseSnapshotName;
			_targetSnapshotName = targetSnapshotName;
			_comparisonResult = comparisonResult;
		}

		private void StatusComparisonResultForm_Load(object sender, System.EventArgs e)
		{
			lblBaseSnapshotName.Text = _baseSnapshotName;
			lblTargetSnapshotName.Text = _targetSnapshotName;

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
			dgvComparisonResult.Columns.Add("Defference", "差分");
		}

		private void displayComparisonResult()
		{
			dgvComparisonResult.Rows.Clear();

			addComparesonResultRow("攻撃力", _comparisonResult.AttackDifference);
			addComparesonResultRow("HP", _comparisonResult.HpDifference);
			addComparesonResultRow("防御力", _comparisonResult.DefenseDifference);
			addComparesonResultRow("素早さ", _comparisonResult.SpeedDifference);
			addComparesonResultRow("会心率", _comparisonResult.CriticalRateDifference);
			addComparesonResultRow("会心DMG", _comparisonResult.CriticalDamageDifference);
			addComparesonResultRow("効果命中", _comparisonResult.EffectHitDifference);
			addComparesonResultRow("効果抵抗", _comparisonResult.EffectResistDifference);

		}

		private void addComparesonResultRow(string statusName, double difference)
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
