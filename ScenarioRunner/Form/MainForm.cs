using ScenarioRunner.Log;
using ScenarioRunner.ScenarioFormat;
using System;
using System.IO;
using System.Windows.Forms;

namespace ScenarioRunner.Form
{
	public partial class MainForm : System.Windows.Forms.Form
	{
		private readonly ScenarioLoader mScenarioLoader;
		private readonly ScenarioLogger mScenarioLogger;

		private Scenario mScenario;

		public MainForm()
		{
			InitializeComponent();

			mScenarioLoader = new ScenarioLoader();

			string logDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");
			mScenarioLogger = new ScenarioLogger(logDirectoryPath);
			mScenarioLogger.LogWritten += appendLog;

			btnRun.Enabled = false;
			btnStop.Enabled = false;
		}

		private void btnBrowseScenario_Click(object sender, EventArgs e)
		{
			using (var dialog = new OpenFileDialog())
			{
				dialog.Title = "Scenarioファイルを選択";
				dialog.Filter = "Scenario File (*.scenario)|*.scenario|All Files (*.*)|*.*";
				dialog.Multiselect = false;

				if (dialog.ShowDialog() != DialogResult.OK)
				{
					return;
				}

				loadScenario(dialog.FileName);
			}
		}

		private void loadScenario(string filePath)
		{
			txtScenarioPath.Text = filePath;
			rtbScenario.Text = File.ReadAllText(filePath);

			rtbExecutionLog.Clear();
			btnRun.Enabled = false;
			mScenario = null;

			try
			{
				mScenario = mScenarioLoader.Load(filePath);

				mScenarioLogger.ScenarioLoaded(mScenario);

				btnRun.Enabled = true;
			}
			catch (Exception ex)
			{
				mScenarioLogger.Error(ex.Message);
			}
		}

		private void appendLog(string message)
		{
			rtbExecutionLog.AppendText(message + Environment.NewLine);
		}
	}
}
