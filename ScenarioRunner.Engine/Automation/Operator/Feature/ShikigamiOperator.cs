using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using ScenarioRunner.Automation.Definition;
using System;

namespace ScenarioRunner.Automation.Operator.Feature
{
	public class ShikigamiOperator
	{
		private readonly ComboBoxOperator mComboBoxOperator;
		private readonly ButtonOperator mButtonOperator;
		private readonly DialogOperator mDialogOperator;
		private readonly GuiOperator mGuiOperator;

		public ShikigamiOperator()
		{
			mComboBoxOperator = new ComboBoxOperator();
			mButtonOperator = new ButtonOperator();
			mDialogOperator = new DialogOperator();
			mGuiOperator = new GuiOperator();
		}

		public void Select(GuiSession session, string shikigamiName)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			AutomationElement shikigamiElement = mGuiOperator.GetMainElement(session, AutomationIds.MainForm.SHIKIGAMI, ControlType.ComboBox);

			mComboBoxOperator.SelectItem(shikigamiElement, shikigamiName);
		}

		public void SelectFirst(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			AutomationElement shikigamiElement = mGuiOperator.GetMainElement(session, AutomationIds.MainForm.SHIKIGAMI, ControlType.ComboBox);

			mComboBoxOperator.SelectFirstItem(shikigamiElement);
		}

		public void ClearSelection(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			AutomationElement button = mGuiOperator.GetMainElement(session, AutomationIds.MainForm.CLEAR_SHIKIGAMI, ControlType.Button);

			mButtonOperator.Click(button);
		}

		public void Check(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			if (mDialogOperator.Exists(session))
			{
				throw new InvalidOperationException("A modal dialog is displayed after reloading shikigami data.");
			}

			AutomationElement shikigamiElement = mGuiOperator.GetMainElement(session, AutomationIds.MainForm.SHIKIGAMI, ControlType.ComboBox);

			if (!mComboBoxOperator.CanSelectFirstItem(shikigamiElement))
			{
				throw new InvalidOperationException("Shikigami ComboBox has no items.");
			}
		}
	}
}
