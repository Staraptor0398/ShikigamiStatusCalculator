using System.Windows.Forms;

namespace ShikigamiApp
{
	public partial class StatusComparisonResultForm : Form
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

			addComparesonResultRow("攻撃力", _comparisonResult.AttackDifferense);
			addComparesonResultRow("HP", _comparisonResult.HpDifferense);
			addComparesonResultRow("防御力", _comparisonResult.DefenseDifferense);
			addComparesonResultRow("素早さ", _comparisonResult.SpeedDifferense);
			addComparesonResultRow("会心率", _comparisonResult.CriticalRateDifferense);
			addComparesonResultRow("会心DMG", _comparisonResult.CriticalDamageDifferense);
			addComparesonResultRow("効果命中", _comparisonResult.EffectHitDifferense);
			addComparesonResultRow("効果抵抗", _comparisonResult.EffectResistDifferense);

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
