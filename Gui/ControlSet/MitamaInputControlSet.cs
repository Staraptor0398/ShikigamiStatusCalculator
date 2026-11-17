using Gui.Form.Control;
using System;
using System.Windows.Forms;

namespace Gui.ControlSet
{
	internal sealed class MitamaInputControlSet
	{
		public MitamaSlotInputControl[] Slots { get; }
		public ComboBox[] SetEffects { get; }
		public ComboBox[] UniqueEffects { get; }

		public MitamaInputControlSet(MitamaSlotInputControl[] slots, ComboBox[] setEffects, ComboBox[] uniqueEffects)
		{
			if (slots == null)
			{
				throw new ArgumentNullException(nameof(slots));
			}
			if (setEffects == null)
			{
				throw new ArgumentNullException(nameof(setEffects));
			}
			if (uniqueEffects == null)
			{
				throw new ArgumentNullException(nameof(uniqueEffects));
			}

			Slots = slots;
			SetEffects = setEffects;
			UniqueEffects = uniqueEffects;
		}
	}
}
