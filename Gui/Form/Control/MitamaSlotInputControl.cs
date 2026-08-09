using System.Windows.Forms;

namespace Gui.Form.Control
{
	public class MitamaSlotInputControl
	{
		public ComboBox MainStatComboBox { get; set; }
		public TextBox MainValueTextBox { get; set; }
		public SubStatInputControl[] SubStats { get; set; }
	}
}
