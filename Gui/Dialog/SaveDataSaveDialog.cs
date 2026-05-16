using Gui.Common;
using System;
using System.IO;
using System.Windows.Forms;

namespace Gui.Dialog
{
	public partial class SaveDataSaveDialog : Form
	{
		private readonly string _shikigamiName;
		private readonly bool _canSaveCalculationSnapshot;

		public SaveDataSaveType _selectedSaveType { get; private set; }

		public string _filePath { get; private set; }

		public SaveDataSaveDialog(string shikigamiName, bool canSaveCalculationSnapshot)
		{
			InitializeComponent();

			_shikigamiName = shikigamiName;
			_canSaveCalculationSnapshot = canSaveCalculationSnapshot;
		}

		private void initializeSaveTypeComboBox()
		{
			cmbSaveType.Items.Clear();

			cmbSaveType.Items.Add(getSaveTypeDisplayText(SaveDataSaveType.Build));
			cmbSaveType.Items.Add(getSaveTypeDisplayText(SaveDataSaveType.MitamaSet));

			if (_canSaveCalculationSnapshot)
			{
				cmbSaveType.Items.Add(getSaveTypeDisplayText(SaveDataSaveType.CalculationSnapshot));
			}

			cmbSaveType.SelectedIndex = 0;
		}

		private string getSaveTypeDisplayText(SaveDataSaveType saveType)
		{
			string text = "";

			switch (saveType)
			{
				case SaveDataSaveType.Build:
					text = "ビルド保存データ";
					break;
				case SaveDataSaveType.MitamaSet:
					text = "御魂セット保存データ";
					break;
				case SaveDataSaveType.CalculationSnapshot:
					text = "計算結果スナップショット";
					break;
			}

			return text;
		}

		private SaveDataSaveType getSelectedSaveType()
		{
			SaveDataSaveType selectedSaveType = SaveDataSaveType.Build;

			string selectedText = cmbSaveType.Text;

			if (selectedText == getSaveTypeDisplayText(SaveDataSaveType.Build))
			{
				selectedSaveType = SaveDataSaveType.Build;
			}
			else if (selectedText == getSaveTypeDisplayText(SaveDataSaveType.MitamaSet))
			{
				selectedSaveType = SaveDataSaveType.MitamaSet;
			}
			else if (selectedText == getSaveTypeDisplayText(SaveDataSaveType.CalculationSnapshot))
			{
				selectedSaveType = SaveDataSaveType.CalculationSnapshot;
			}

			return selectedSaveType;
		}

		private string getSaveTypeExtension(SaveDataSaveType saveType)
		{
			string extension = "";

			switch (saveType)
			{
				case SaveDataSaveType.Build:
					extension = SaveDataFileDefinition.BuildExtension;
					break;
				case SaveDataSaveType.MitamaSet:
					extension = SaveDataFileDefinition.MitamaSetExtension;
					break;
				case SaveDataSaveType.CalculationSnapshot:
					extension = SaveDataFileDefinition.SnapshotExtension;
					break;
			}

			return extension;
		}

		private string getSaveTypeFilter(SaveDataSaveType saveType)
		{
			string filter = "";

			switch (saveType)
			{
				case SaveDataSaveType.Build:
					filter = SaveDataFileDefinition.BuildFilter;
					break;
				case SaveDataSaveType.MitamaSet:
					filter = SaveDataFileDefinition.MitamaSetFilter;
					break;
				case SaveDataSaveType.CalculationSnapshot:
					filter = SaveDataFileDefinition.SnapshotFilter;
					break;
			}

			return filter;
		}

		private string getSaveTypeDirectoryPath(SaveDataSaveType saveType)
		{
			string directoryPath = "";

			switch (saveType)
			{
				case SaveDataSaveType.Build:
					directoryPath = AppPath.BuildSaveDataDirectoryPath;
					break;
				case SaveDataSaveType.MitamaSet:
					directoryPath = AppPath.MitamaSetSaveDataDirectoryPath;
					break;
				case SaveDataSaveType.CalculationSnapshot:
					directoryPath = AppPath.SnapshotSaveDataDirectoryPath;
					break;
			}

			return directoryPath;
		}

		private string createDefaultFileName(SaveDataSaveType saveType)
		{
			string fileName = "";

			switch (saveType)
			{
				case SaveDataSaveType.Build:
					if (string.IsNullOrWhiteSpace(_shikigamiName))
					{
						fileName = "Build";
					}
					else
					{
						fileName = _shikigamiName;
					}
					break;
				case SaveDataSaveType.MitamaSet:
					fileName = "MitamaSet";
					break;
				case SaveDataSaveType.CalculationSnapshot:
					string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

					if (string.IsNullOrWhiteSpace(_shikigamiName))
					{
						fileName = timestamp;
					}
					else
					{
						fileName = _shikigamiName + "_" + timestamp;
					}
					break;
			}

			fileName += getSaveTypeExtension(saveType);

			return fileName;
		}

		private string createDefaultFilePath(SaveDataSaveType saveType)
		{
			return Path.Combine(
				getSaveTypeDirectoryPath(saveType),
				createDefaultFileName(saveType));
		}

		private void SaveDataSaveDialog_Load(object sender, System.EventArgs e)
		{
			initializeSaveTypeComboBox();
		}

		private void cmbSaveType_SelectedIndexChanged(object sender, EventArgs e)
		{
			txtFilePath.Text = createDefaultFilePath(getSelectedSaveType());
		}

		private void btnBrowse_Click(object sender, EventArgs e)
		{
			SaveDataSaveType saveType = getSelectedSaveType();

			using (var dialog = new SaveFileDialog())
			{
				dialog.Filter = getSaveTypeFilter(saveType);
				dialog.InitialDirectory = getSaveTypeDirectoryPath(saveType);
				dialog.FileName = createDefaultFileName(saveType);

				if (dialog.ShowDialog() != DialogResult.OK)
				{
					return;
				}

				txtFilePath.Text = dialog.FileName;
			}
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(txtFilePath.Text))
			{
				MessageBox.Show(
					"保存先ファイルを指定してください。",
					"SaveData保存",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);

				return;
			}

			_selectedSaveType = getSelectedSaveType();
			_filePath = txtFilePath.Text;

			DialogResult = DialogResult.OK;
			this.Close();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			this.Close();
		}
	}
}
