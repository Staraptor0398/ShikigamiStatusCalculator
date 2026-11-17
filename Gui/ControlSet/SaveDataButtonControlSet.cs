using System;
using System.Windows.Forms;

namespace Gui.ControlSet
{
	internal sealed class SaveDataButtonControlSet
	{
		public Button SaveButton { get; }
		public Button LoadButton { get; }

		public SaveDataButtonControlSet(Button saveButton, Button loadButton)
		{
			if (saveButton == null)
			{
				throw new ArgumentNullException(nameof(saveButton));
			}

			if (loadButton == null)
			{
				throw new ArgumentNullException(nameof(loadButton));
			}

			SaveButton = saveButton;
			LoadButton = loadButton;
		}
	}
}
