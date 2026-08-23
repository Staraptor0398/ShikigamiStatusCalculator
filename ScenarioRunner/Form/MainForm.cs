using ScenarioRunner.Execution;
using ScenarioRunner.Log;
using ScenarioRunner.ScenarioFormat;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScenarioRunner.Form
{
	public partial class MainForm : System.Windows.Forms.Form
	{
		private readonly string mGuiExucutablePath;

		private readonly ScenarioLoader mScenarioLoader;
		private readonly ScenarioLogger mScenarioLogger;
		private readonly ScenarioExecutor mScenarioExecutor;

		private Scenario mScenario;

		public MainForm()
		{
			InitializeComponent();

			mGuiExucutablePath = @"W:\Gui\bin\x64\Debug\Gui.exe";

			mScenarioLoader = new ScenarioLoader();

			string logDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");
			mScenarioLogger = new ScenarioLogger(logDirectoryPath);
			mScenarioLogger.LogWritten += appendLog;

			mScenarioExecutor = new ScenarioExecutor(mScenarioLogger, mGuiExucutablePath);

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
			if (rtbExecutionLog.InvokeRequired)
			{
				rtbExecutionLog.Invoke(new Action<string>(appendLog), message);
				return;
			}

			rtbExecutionLog.AppendText(message + Environment.NewLine);
		}

		private async void btnRun_Click(object sender, EventArgs e)
		{
			if (mScenario == null)
			{
				return;
			}

			var options = new ScenarioExecutionOptions(chkWatchMode.Checked, chkKeepGuiOpenOnFailure.Checked);

			btnRun.Enabled = false;
			btnStop.Enabled = true;

			try
			{
				await Task.Run(() => mScenarioExecutor.Execute(mScenario, options));
			}
			finally
			{
				btnRun.Enabled = true;
				btnStop.Enabled = false;
			}
		}

		private void btnShikigamiDataMonitor_Click(object sender, EventArgs e)
		{
			string guiDirectoryPath = Path.GetDirectoryName(mGuiExucutablePath);

			string shikigamiDataPath = Path.Combine(guiDirectoryPath, "Data", "ShikigamiData.csv");

			var monitorForm = new ShikigamiDataMonitorForm(shikigamiDataPath);

			monitorForm.Show();
		}
	}
}
