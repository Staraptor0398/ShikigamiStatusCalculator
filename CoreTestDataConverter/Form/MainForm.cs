namespace CoreTestDataConverter.Form
{
	public partial class MainForm : System.Windows.Forms.Form
	{
		public MainForm()
		{
			InitializeComponent();
		}

		private void btnCalculation_Click(object sender, System.EventArgs e)
		{
			using (var form = new CalculationTestDataForm())
			{
				form.ShowDialog(this);
			}
		}
	}
}
