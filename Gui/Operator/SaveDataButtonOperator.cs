using Gui.ControlSet;
using System;

namespace Gui.Operator
{
	internal sealed class SaveDataButtonOperator
	{
		private readonly SaveDataButtonControlSet mControls;

		public SaveDataButtonOperator(SaveDataButtonControlSet controls)
		{
			if (controls == null)
			{
				throw new ArgumentNullException(nameof(controls));
			}

			mControls = controls;
		}

		public void SetSaveEnabled(bool enabled)
		{
			mControls.SaveButton.Enabled = enabled;
		}

		public void SetLoadEnabled(bool enabled)
		{
			mControls.LoadButton.Enabled = enabled;
		}
	}
}
