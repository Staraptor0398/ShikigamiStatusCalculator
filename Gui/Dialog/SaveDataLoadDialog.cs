using Gui.Common;
using System.Windows.Forms;

namespace Gui.Dialog
{
	public partial class SaveDataLoadDialog : Form
	{
		public SaveDataLoadType _selectedLoadType { get; private set; }

		public string _filePath { get; private set; }

		public SaveDataLoadDialog()
		{
			InitializeComponent();
		}

		private void SaveDataLoadDialog_Load(object sender, System.EventArgs e)
		{
			initializeLoadTypeComboBox();
		}

		private void initializeLoadTypeComboBox()
		{
			cmbLoadType.Items.Clear();

			cmbLoadType.Items.Add(getLoadTypeDisplayText(SaveDataLoadType.Build));
			cmbLoadType.Items.Add(getLoadTypeDisplayText(SaveDataLoadType.MitamaSet));

			cmbLoadType.SelectedIndex = 0;
		}

		private string getLoadTypeDisplayText(SaveDataLoadType loadType)
		{
			string text = "";

			switch (loadType)
			{
				case SaveDataLoadType.Build:
					text = "ビルド保存データ";
					break;
				case SaveDataLoadType.MitamaSet:
					text = "御魂セット保存データ";
					break;
			}

			return text;
		}

		private SaveDataLoadType getSelectedLoadType()
		{
			SaveDataLoadType selectedLoadType = SaveDataLoadType.Build;

			string selectedText = cmbLoadType.Text;

			if (selectedText == getLoadTypeDisplayText(SaveDataLoadType.Build))
			{
				selectedLoadType = SaveDataLoadType.Build;
			}
			else if (selectedText == getLoadTypeDisplayText(SaveDataLoadType.MitamaSet))
			{
				selectedLoadType = SaveDataLoadType.MitamaSet;
			}

			return selectedLoadType;
		}

		private string getLoadTypeFilter(SaveDataLoadType loadType)
		{
			string filter = "";

			switch (loadType)
			{
				case SaveDataLoadType.Build:
					filter = SaveDataFileDefinition.BuildFilter;
					break;
				case SaveDataLoadType.MitamaSet:
					filter = SaveDataFileDefinition.MitamaSetFilter;
					break;
			}

			return filter;
		}

		private string getLoadTypeDirectoryPath(SaveDataLoadType loadType)
		{
			string directoryPath = "";

			switch (loadType)
			{
				case SaveDataLoadType.Build:
					directoryPath = AppPath.BuildSaveDataDirectoryPath;
					break;
				case SaveDataLoadType.MitamaSet:
					directoryPath = AppPath.MitamaSetSaveDataDirectoryPath;
					break;
			}

			return directoryPath;
		}

		private void cmbLoadType_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			txtFilePath.Text = "";
		}

		private void btnBrowse_Click(object sender, System.EventArgs e)
		{
			SaveDataLoadType loadType = getSelectedLoadType();

			using (var dialog = new OpenFileDialog())
			{
				dialog.Filter = getLoadTypeFilter(loadType);
				dialog.InitialDirectory = getLoadTypeDirectoryPath(loadType);

				if (dialog.ShowDialog() != DialogResult.OK)
				{
					return;
				}

				txtFilePath.Text = dialog.FileName;
			}
		}

		private void btnLoad_Click(object sender, System.EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(txtFilePath.Text))
			{
				MessageBox.Show(
					"読み込み元ファイルを指定してください。",
					"SaveData読み込み",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);

				return;
			}

			_selectedLoadType = getSelectedLoadType();
			_filePath = txtFilePath.Text;

			DialogResult = DialogResult.OK;
			this.Close();
		}

		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			this.Close();
		}
	}
}
