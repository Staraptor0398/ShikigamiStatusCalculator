using ScenarioRunner.Automation.Layout;
using ScenarioRunner.Automation.Model;
using ScenarioRunner.Execution;
using ScenarioRunner.Form.Applicator;
using ScenarioRunner.Log;
using ScenarioRunner.Presentation;
using ScenarioRunner.ScenarioFormat;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScenarioRunner.Form
{
	public partial class MainForm : System.Windows.Forms.Form
	{
		private readonly string mGuiExucutablePath;

		private readonly ScenarioLoader mScenarioLoader;
		private readonly ScenarioCompiler mScenarioCompiler;

		private readonly ScenarioLogger mScenarioLogger;
		private readonly ScenarioExecutor mScenarioExecutor;

		private readonly ScenarioHighlighter mScenarioHighlighter;

		private ScenarioWindowLayout mWindowLayout;
		private Scenario mScenario;

		private bool mIsEditMode;

		public MainForm()
		{
			InitializeComponent();

			Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;

			WindowBounds workingAreaBounds = new WindowBounds(workingArea.X, workingArea.Y, workingArea.Width, workingArea.Height);

			mWindowLayout = new ScenarioWindowLayout(workingAreaBounds);

			mGuiExucutablePath = @"W:\Gui\bin\x64\Debug\Gui.exe";

			mScenarioLoader = new ScenarioLoader();
			mScenarioCompiler = new ScenarioCompiler();

			string logDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");
			mScenarioLogger = new ScenarioLogger(logDirectoryPath);
			mScenarioLogger.LogWritten += appendLog;

			mScenarioHighlighter = new ScenarioHighlighter(rtbScenario);

			mScenarioLogger.StepStartedEvent += mScenarioHighlighter.ShowRunning;
			mScenarioLogger.StepStartedEvent += scrollScenario;
			mScenarioLogger.StepPassedEvent += mScenarioHighlighter.ShowPassed;
			mScenarioLogger.StepFailedEvent += mScenarioHighlighter.ShowFailed;

			mScenarioExecutor = new ScenarioExecutor(mScenarioLogger, mGuiExucutablePath, mWindowLayout.GuiBounds);

			setEditMode(false);
		}

		private void btnBrowseScenario_Click(object sender, EventArgs e)
		{
			if (mIsEditMode)
			{
				selectScenarioSavePath();
				return;
			}

			selectScenarioOpenPath();
		}

		private void selectScenarioSavePath()
		{
			using (var dialog = new SaveFileDialog())
			{
				dialog.Title = "Scenarioファイルの保存先を選択";
				dialog.Filter = "Scenario File (*.scenario)|*.scenario|All Files (*.*)|*.*";
				dialog.FileName = Path.GetFileName(txtScenarioPath.Text);

				if (dialog.ShowDialog() != DialogResult.OK)
				{
					return;
				}

				txtScenarioPath.Text = dialog.FileName;
			}
		}

		private void selectScenarioOpenPath()
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
				mScenarioLogger.ScenarioValidationStarted(filePath);

				mScenario = mScenarioLoader.Load(filePath);

				mScenarioLogger.ScenarioLoaded(mScenario);

				mScenarioHighlighter.ApplySyntax(mScenario);

				setEditMode(false);
			}
			catch (ScenarioValidationException ex)
			{
				mScenarioLogger.ScenarioValidationFailed(ex.Message);
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

			RichTextBoxScrollApplicator.Apply(rtbExecutionLog, rtbExecutionLog.Lines.Length - 1);
		}

		private void scrollScenario(ScenarioStep step)
		{
			if (rtbScenario.InvokeRequired)
			{
				rtbScenario.Invoke(new Action<ScenarioStep>(scrollScenario), step);
				return;
			}

			RichTextBoxScrollApplicator.Apply(rtbScenario, step.LineNumber - 1);
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
				mScenarioHighlighter.ResetExecutionState();
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

			FormBoundsApplicator.Apply(monitorForm, mWindowLayout.MonitorBounds);
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			FormBoundsApplicator.Apply(this, mWindowLayout.RunnerBounds);
		}

		private void btnEdit_Click(object sender, EventArgs e)
		{
			setEditMode(true);
		}

		private void setEditMode(bool isEditMode)
		{
			mIsEditMode = isEditMode;

			rtbScenario.ReadOnly = !isEditMode;

			btnEdit.Enabled = !isEditMode;
			btnSave.Enabled = isEditMode;
			btnRun.Enabled = !isEditMode && mScenario != null;
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			string filePath = txtScenarioPath.Text;

			if (string.IsNullOrWhiteSpace(filePath))
			{
				MessageBox.Show(
					"保存先のScenarioファイルを指定してください。",
					"Scenario Save",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);

				return;
			}

			try
			{
				mScenarioCompiler.Compile(filePath, rtbScenario.Lines);

				File.WriteAllText(filePath, rtbScenario.Text);

				loadScenario(filePath);
			}
			catch (ScenarioValidationException ex)
			{
				MessageBox.Show(
					ex.Message,
					"Scenario Validation Error",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
			}
			catch (Exception ex)
			{
				MessageBox.Show(
					ex.Message,
					"Scenario Save Error",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
			}
		}
	}
}
