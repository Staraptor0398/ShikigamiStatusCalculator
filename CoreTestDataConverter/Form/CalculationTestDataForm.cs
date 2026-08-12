using CoreTestDataConverter.Access;
using CoreTestDataConverter.Converter.MitamaCalculator;
using CoreTestDataConverter.Converter.StatusCalculator;
using CoreTestDataConverter.Output;
using System;
using System.IO;
using System.Windows.Forms;

namespace CoreTestDataConverter.Form
{
	public partial class CalculationTestDataForm : System.Windows.Forms.Form
	{
		private const string MitamaCalculatorDataDirectory = @"W:\CoreTest\TestCase\MitamaCalculator\_data";

		private const string StatusCalculatorDataDirectory = @"W:\CoreTest\TestCase\StatusCalculator\_data";

		public CalculationTestDataForm()
		{
			InitializeComponent();

			txtTestSourcePath.ReadOnly = true;
			txtMitamaCalculatorOutputPath.ReadOnly = true;
			txtStatusCalculatorOutputPath.ReadOnly = true;

			btnGenerate.Enabled = false;
		}

		private void btnBrowseTestSource_Click(object sender, EventArgs e)
		{
			using (var dialog = new OpenFileDialog())
			{
				dialog.InitialDirectory = @"W:\TestSource\Calculation";
				dialog.Filter = "JSON files (*.json)|*.json";

				if (dialog.ShowDialog() != DialogResult.OK)
				{
					return;
				}

				txtTestSourcePath.Text = dialog.FileName;

				UpdateOutputPaths();

				btnGenerate.Enabled = true;
			}
		}

		private void UpdateOutputPaths()
		{
			var mitamaFileName = TestDataFileNameGenerator.GenerateNext(MitamaCalculatorDataDirectory);
			var statusFileName = TestDataFileNameGenerator.GenerateNext(StatusCalculatorDataDirectory);

			txtMitamaCalculatorOutputPath.Text = Path.Combine(MitamaCalculatorDataDirectory, mitamaFileName);
			txtStatusCalculatorOutputPath.Text = Path.Combine(StatusCalculatorDataDirectory, statusFileName);
		}

		private void btnGenerate_Click(object sender, EventArgs e)
		{
			var source = TestDataAccess.LoadCalculationTestSource(txtTestSourcePath.Text);
			var mitamaTestData = MitamaCalculatorTestDataConverter.ToTestData(source);
			var statusTestData = StatusCalculatorTestDataConverter.ToTestData(source);

			TestDataAccess.SaveMitamaCalculatorTestData(txtMitamaCalculatorOutputPath.Text, mitamaTestData);
			TestDataAccess.SaveStatusCalculatorTestData(txtStatusCalculatorOutputPath.Text, statusTestData);

			MessageBox.Show(
				"TestDataを生成しました。",
				"CoreTestDataConverter",
				MessageBoxButtons.OK,
				MessageBoxIcon.Information);

			UpdateOutputPaths();
		}
	}
}
