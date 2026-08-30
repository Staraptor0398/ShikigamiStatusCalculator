using FlaUI.Core.AutomationElements;
using ScenarioRunner.Automation.Definition;
using System;
using System.Collections.Generic;

namespace ScenarioRunner.Automation.Operator
{
	public class InputOperator
	{
		private readonly ButtonOperator mButtonOperator;
		private readonly ComboBoxOperator mComboBoxOperator;
		private readonly DialogOperator mDialogOperator;
		private readonly GuiOperator mGuiOperator;
		private readonly TextBoxOperator mTextBoxOperator;

		public InputOperator()
		{
			mButtonOperator = new ButtonOperator();
			mComboBoxOperator = new ComboBoxOperator();
			mDialogOperator = new DialogOperator();
			mGuiOperator = new GuiOperator();
			mTextBoxOperator = new TextBoxOperator();
		}

		public void Equip(GuiSession session, IReadOnlyList<string> arguments)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}
			if (arguments == null)
			{
				throw new ArgumentNullException(nameof(arguments));
			}

			Window mainWindow = mGuiOperator.GetMainWindow(session);

			switch (arguments[0])
			{
				case "MAIN":
					equipMain(mainWindow, arguments);
					break;
				case "SUB":
					equipSub(mainWindow, arguments);
					break;
				case "SET":
					equipSet(mainWindow, arguments);
					break;
				case "UNIQUE":
					equipUnique(mainWindow, arguments);
					break;
				default:
					throw new InvalidOperationException($"Unknown EQUIP MITAMA target: {arguments[0]}");
			}
		}

		public void Clear(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			Window mainWindow = mGuiOperator.GetMainWindow(session);

			mButtonOperator.Click(mainWindow, AutomationIds.MainForm.CLEAR);

			Window dialog = mDialogOperator.GetActiveDialog(session);

			if (dialog == null)
			{
				throw new InvalidOperationException("Clear confirmation dialog was not found.");
			}

			mButtonOperator.Click(dialog, AutomationIds.MessageBox.YES);
		}

		private void equipMain(Window mainWindow, IReadOnlyList<string> arguments)
		{
			int mitamaSlot = int.Parse(arguments[1]);
			string statType = arguments[2];

			mComboBoxOperator.SelectItem(mainWindow, AutomationIds.MainForm.MainStat(mitamaSlot), statType);
		}

		private void equipSub(Window mainWindow, IReadOnlyList<string> arguments)
		{
			int mitamaSlot = int.Parse(arguments[1]);
			int subSlot = int.Parse(arguments[2]);

			if (arguments.Count >= 4 && !string.IsNullOrEmpty(arguments[3]))
			{
				mComboBoxOperator.SelectItem(mainWindow, AutomationIds.MainForm.SubStat(mitamaSlot, subSlot), arguments[3]);
			}

			if (arguments.Count >= 5)
			{
				mTextBoxOperator.SetText(mainWindow, AutomationIds.MainForm.SubStatValue(mitamaSlot, subSlot), arguments[4]);
			}
		}

		private void equipSet(Window mainWindow, IReadOnlyList<string> arguments)
		{
			int slot = int.Parse(arguments[1]);
			string statType = arguments[2];

			mComboBoxOperator.SelectItem(mainWindow, AutomationIds.MainForm.SetEffect(slot), statType);
		}

		private void equipUnique(Window mainWindow, IReadOnlyList<string> arguments)
		{
			int slot = int.Parse(arguments[1]);
			string statType = arguments[2];

			mComboBoxOperator.SelectItem(mainWindow, AutomationIds.MainForm.UniqueEffect(slot), statType);
		}
	}
}
