using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using ScenarioRunner.Automation.Definition;
using System;
using System.Collections.Generic;

namespace ScenarioRunner.Automation.Operator.Feature
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

			AutomationElementMap elementMap = getElementMap(session);

			switch (arguments[0])
			{
				case "MAIN":
					equipMain(elementMap, arguments);
					break;
				case "SUB":
					equipSub(elementMap, arguments);
					break;
				case "SET":
					equipSet(elementMap, arguments);
					break;
				case "UNIQUE":
					equipUnique(elementMap, arguments);
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

			AutomationElement clearButton = mGuiOperator.GetMainElement(session, AutomationIds.MainForm.CLEAR, ControlType.Button);

			mButtonOperator.Click(clearButton);
		}

		public void CheckCleared(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			AutomationElementMap elementMap = getElementMap(session);

			checkComboBoxCleared(elementMap, AutomationIds.MainForm.SHIKIGAMI);
			checkTextBoxCleared(elementMap, AutomationIds.MainForm.BASE_STATS);

			for (int mitamaSlot = 1; mitamaSlot <= 6; mitamaSlot++)
			{
				checkComboBoxCleared(elementMap, AutomationIds.MainForm.MainStat(mitamaSlot));
				checkTextBoxCleared(elementMap, AutomationIds.MainForm.MainStatValue(mitamaSlot));

				for (int subSlot = 1; subSlot <= 4; subSlot++)
				{
					checkComboBoxCleared(elementMap, AutomationIds.MainForm.SubStat(mitamaSlot, subSlot));
					checkTextBoxCleared(elementMap, AutomationIds.MainForm.SubStatValue(mitamaSlot, subSlot));
				}
			}

			for (int slot = 1; slot <= 3; slot++)
			{
				checkComboBoxCleared(elementMap, AutomationIds.MainForm.SetEffect(slot));
			}

			for (int slot = 1; slot <= 6; slot++)
			{
				checkComboBoxCleared(elementMap, AutomationIds.MainForm.UniqueEffect(slot));
			}

			checkTextBoxCleared(elementMap, AutomationIds.MainForm.MITAMA_ONLY);
			checkTextBoxCleared(elementMap, AutomationIds.MainForm.FINAL_STATS);
		}

		private void equipMain(AutomationElementMap elementMap, IReadOnlyList<string> arguments)
		{
			int mitamaSlot = int.Parse(arguments[1]);
			string statType = arguments[2];

			AutomationElement element = elementMap.Get(AutomationIds.MainForm.MainStat(mitamaSlot), ControlType.ComboBox);

			mComboBoxOperator.SelectItem(element, statType);
		}

		private void equipSub(AutomationElementMap elementMap, IReadOnlyList<string> arguments)
		{
			int mitamaSlot = int.Parse(arguments[1]);
			int subSlot = int.Parse(arguments[2]);

			if (arguments.Count >= 4 && !string.IsNullOrEmpty(arguments[3]))
			{
				AutomationElement comboBoxElement = elementMap.Get(AutomationIds.MainForm.SubStat(mitamaSlot, subSlot), ControlType.ComboBox);

				mComboBoxOperator.SelectItem(comboBoxElement, arguments[3]);
			}

			if (arguments.Count >= 5)
			{
				AutomationElement textBoxElement =
					elementMap.Get(AutomationIds.MainForm.SubStatValue(mitamaSlot, subSlot));

				mTextBoxOperator.SetText(textBoxElement, arguments[4]);
			}
		}

		private void equipSet(AutomationElementMap elementMap, IReadOnlyList<string> arguments)
		{
			int slot = int.Parse(arguments[1]);
			string statType = arguments[2];

			AutomationElement element = elementMap.Get(AutomationIds.MainForm.SetEffect(slot), ControlType.ComboBox);

			mComboBoxOperator.SelectItem(element, statType);
		}

		private void equipUnique(AutomationElementMap elementMap, IReadOnlyList<string> arguments)
		{
			int slot = int.Parse(arguments[1]);
			string statType = arguments[2];

			AutomationElement element = elementMap.Get(AutomationIds.MainForm.UniqueEffect(slot), ControlType.ComboBox);

			mComboBoxOperator.SelectItem(element, statType);
		}

		private void checkComboBoxCleared(AutomationElementMap elementMap, string automationId)
		{
			AutomationElement element = elementMap.Get(automationId, ControlType.ComboBox);
			string value = mComboBoxOperator.GetValue(element);

			if (!string.IsNullOrWhiteSpace(value))
			{
				throw new InvalidOperationException($"ComboBox was not cleared: {automationId}, value={value}");
			}
		}

		private void checkTextBoxCleared(AutomationElementMap elementMap, string automationId)
		{
			AutomationElement element = elementMap.Get(automationId);
			string text = mTextBoxOperator.GetText(element);

			if (!string.IsNullOrWhiteSpace(text))
			{
				throw new InvalidOperationException($"TextBox was not cleared: {automationId}, value={text}");
			}
		}

		private AutomationElementMap getElementMap(GuiSession session)
		{
			return mGuiOperator.GetMainElementMap(session);
		}
	}
}
