using Gui.Common;
using System.Windows.Forms;

namespace Gui.Dialog
{
	public partial class SnapshotCompareFileSelectDialog : Form
	{

		public string _baseSnapshotFilePath { get; private set; }

		public string _targetSnapshotFilePath { get; private set; }

		public SnapshotCompareFileSelectDialog()
		{
			InitializeComponent();
		}

		private void SnapshotCompareFileSelectDialog_Load(object sender, System.EventArgs e)
		{
			updateCompareButtonEnabled();
		}

		private void btnBrowseBaseSnapshot_Click(object sender, System.EventArgs e)
		{
			string filePath = selectSnapshotFile();

			if (string.IsNullOrWhiteSpace(filePath))
			{
				return;
			}

			txtBaseSnapshotFilePath.Text = filePath;

			updateCompareButtonEnabled();
		}

		private void btnBrowseTargetSnapshot_Click(object sender, System.EventArgs e)
		{
			string filePath = selectSnapshotFile();

			if (string.IsNullOrWhiteSpace(filePath))
			{
				return;
			}

			txtTargetSnapshotFilePath.Text = filePath;

			updateCompareButtonEnabled();
		}

		private string selectSnapshotFile()
		{
			using (var dialog = new OpenFileDialog())
			{
				dialog.Filter = SaveDataFileDefinition.SnapshotFilter;
				dialog.InitialDirectory = AppPath.SnapshotSaveDataDirectoryPath;

				if (dialog.ShowDialog() != DialogResult.OK)
				{
					return "";
				}

				return dialog.FileName;
			}
		}

		private void updateCompareButtonEnabled()
		{
			btnCompare.Enabled = !string.IsNullOrWhiteSpace(txtBaseSnapshotFilePath.Text) && !string.IsNullOrWhiteSpace(txtTargetSnapshotFilePath.Text);
		}

		private void btnCompare_Click(object sender, System.EventArgs e)
		{
			if (string.Equals(txtBaseSnapshotFilePath.Text, txtTargetSnapshotFilePath.Text))
			{
				MessageBox.Show(
					"同じスナップショットファイルは比較できません。",
					"スナップショット比較",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);

				return;
			}

			_baseSnapshotFilePath = txtBaseSnapshotFilePath.Text;
			_targetSnapshotFilePath = txtTargetSnapshotFilePath.Text;

			DialogResult = DialogResult.OK;
			this.Close();
		}

		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			_baseSnapshotFilePath = "";
			_targetSnapshotFilePath = "";

			DialogResult = DialogResult.Cancel;
			this.Close();
		}
	}
}
