using System.IO;

namespace ScenarioRunner.Form
{
	public partial class ShikigamiDataMonitorForm : System.Windows.Forms.Form
	{
		private readonly string mFilePath;

		public ShikigamiDataMonitorForm(string filePath)
		{
			InitializeComponent();

			mFilePath = filePath;
			txtFilePath.Text = mFilePath;


		}

		private void refreshContent()
		{
			if (!File.Exists(mFilePath))
			{
				rtbContent.Text = "ShikigamiData.csv was not found.";
				return;
			}

			try
			{
				rtbContent.Text = File.ReadAllText(mFilePath);
			}
			catch (IOException ex)
			{
				rtbContent.Text = $"Failed to read file.\r\n{ex.Message}";
			}
		}

		private void btnRefresh_Click(object sender, System.EventArgs e)
		{
			refreshContent();
		}

		private void tmrRefresh_Tick(object sender, System.EventArgs e)
		{
			if (chkAutoRefresh.Checked)
			{
				refreshContent();
			}
		}
	}
}
