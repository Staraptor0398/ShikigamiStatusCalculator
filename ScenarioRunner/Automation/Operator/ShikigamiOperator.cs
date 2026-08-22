using System;

namespace ScenarioRunner.Automation.Operator
{
	public class ShikigamiOperator
	{
		private const string SHIKIGAMI_COMBO_BOX_AUTOMATION_ID = "cmbShikigami";

		private readonly ComboBoxOperator mComboBoxOperator;

		public ShikigamiOperator()
		{
			mComboBoxOperator = new ComboBoxOperator();
		}

		public void Select(GuiSession session, string shikigamiName)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			mComboBoxOperator.SelectItem(session.MainWindow, SHIKIGAMI_COMBO_BOX_AUTOMATION_ID, shikigamiName);
		}
	}
}
