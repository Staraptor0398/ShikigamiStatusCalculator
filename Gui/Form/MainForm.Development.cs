#if DEBUG
using Gui.Common;
using SaveData.Access.Development;
using System;
using System.Windows.Forms;

namespace Gui.Form
{
	public partial class MainForm
	{
		private Button btnExportCalculationTestSource;

		private void initializeDevelopmentControls()
		{
			btnExportCalculationTestSource = new Button()
			{
				Location = new System.Drawing.Point(31, 320),
				Name = "btnExportCalculationTestSource",
				Size = new System.Drawing.Size(92, 23),
				TabIndex = 97,
				Text = "テストソース出力",
				UseVisualStyleBackColor = true
			};

			btnExportCalculationTestSource.Click += btnExportCalculationTestSource_Click;
			Controls.Add(btnExportCalculationTestSource);
		}

		private void btnExportCalculationTestSource_Click(object sender, EventArgs e)
		{
			if (_lastCalculationTestSource == null)
			{
				MessageBox.Show(
					"出力できる計算結果がありません。",
					"テストソース出力",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);

				return;
			}

			using (SaveFileDialog dialog = new SaveFileDialog())
			{
				dialog.Filter = "JSONファイル (*.json)|*.json";
				dialog.InitialDirectory = AppPath.CALCULATION_TEST_SOURCE_DIRECTORY_PATH;
				dialog.FileName = $"CalculationTestSource_{DateTime.Now:yyyyMMdd_HHmmss}.json";

				if (dialog.ShowDialog() != DialogResult.OK)
				{
					return;
				}
				try
				{
					CalculationTestSourceAccess.Save(dialog.FileName, _lastCalculationTestSource);
				}
				catch (Exception ex)
				{
					Logger.Error($"Operation=計算テストソース出力 Message={ex}");

					MessageBox.Show(
						"テストソースの出力中に予期しないエラーが発生しました。",
						"テストソース出力",
						MessageBoxButtons.OK,
						MessageBoxIcon.Error);

					return;
				}

				MessageBox.Show(
					"テストソースを出力しました。",
					"テストソース出力",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
			}
		}
	}
}

#endif
