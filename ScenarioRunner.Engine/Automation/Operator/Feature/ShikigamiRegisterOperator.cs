using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using ScenarioRunner.Automation.Definition;
using ScenarioRunner.Automation.Waiter;
using ScenarioRunner.Execution;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace ScenarioRunner.Automation.Operator.Feature
{
	public class ShikigamiRegisterOperator
	{
		private static readonly string[] mRequiredFields =
		{
			"Rarity",
			"Name",
			"Attack",
			"HP",
			"Defense",
			"Speed",
			"CritRate",
			"CritDamage",
			"EffectHit",
			"EffectResist"
		};

		private readonly ButtonOperator mButtonOperator;
		private readonly ComboBoxOperator mComboBoxOperator;
		private readonly TextBoxOperator mTextBoxOperator;
		private readonly GuiOperator mGuiOperator;

		private readonly ProcessWindowWaiter mProcessWindowWaiter;

		public ShikigamiRegisterOperator()
		{
			mButtonOperator = new ButtonOperator();
			mComboBoxOperator = new ComboBoxOperator();
			mTextBoxOperator = new TextBoxOperator();
			mGuiOperator = new GuiOperator();

			mProcessWindowWaiter = new ProcessWindowWaiter();
		}

		public void Open(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			AutomationElement button = mGuiOperator.GetMainElement(session, AutomationIds.MainForm.REGISTER_SHIKIGAMI, ControlType.Button);

			mButtonOperator.PostClick(button);
		}

		public void Register(ScenarioExecutonContext context, string filePath)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			if (string.IsNullOrWhiteSpace(filePath))
			{
				throw new ArgumentException("Shikigami input data file path is empty.", nameof(filePath));
			}

			GuiSession session = context.GuiSession;
			string resolvedFilePath = context.ResolvePath(filePath);

			IDictionary<string, string> inputData = loadInputData(resolvedFilePath);

			Window registerForm = getRegisterForm(session);
			var elementMap = new AutomationElementMap(registerForm);

			applyInputData(elementMap, inputData);

			AutomationElement registerButton = elementMap.Get(AutomationIds.ShikigamiRegisterForm.REGISTER, ControlType.Button);

			mButtonOperator.PostClick(registerButton);
		}

		private Window getRegisterForm(GuiSession session)
		{
			Window mainWindow = mGuiOperator.GetMainWindow(session);

			return mProcessWindowWaiter.WaitForProcessWindow(session, mainWindow, element => element.Properties.AutomationId.ValueOrDefault == AutomationIds.ShikigamiRegisterForm.ID);
		}

		private IDictionary<string, string> loadInputData(string filePath)
		{
			if (!File.Exists(filePath))
			{
				throw new FileNotFoundException("Shikigami input data file was not found.", filePath);
			}

			string json = File.ReadAllText(filePath);
			byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

			var settings = new DataContractJsonSerializerSettings
			{
				UseSimpleDictionaryFormat = true
			};

			var serializer = new DataContractJsonSerializer(typeof(Dictionary<string, string>), settings);

			Dictionary<string, string> inputData;

			using (var stream = new MemoryStream(jsonBytes))
			{
				try
				{
					inputData = serializer.ReadObject(stream) as Dictionary<string, string>;
				}
				catch (Exception ex)
				{
					throw new InvalidOperationException($"Shikigami input data could not be read as JSON: {filePath}", ex);
				}
			}

			if (inputData == null)
			{
				throw new InvalidOperationException($"Shikigami input data could not be read: {filePath}");
			}

			validateInputData(inputData, filePath);

			return inputData;
		}

		private void validateInputData(IDictionary<string, string> inputData, string filePath)
		{
			foreach (string field in mRequiredFields)
			{
				if (!inputData.ContainsKey(field))
				{
					throw new InvalidOperationException($"Required shikigami input field is missing: {field}, File={filePath}");
				}
			}
		}

		private void applyInputData(AutomationElementMap elementMap, IDictionary<string, string> inputData)
		{
			AutomationElement rarityElement = elementMap.Get(AutomationIds.ShikigamiRegisterForm.RARITY, ControlType.ComboBox);

			string rarity = getValue(inputData, "Rarity");

			if (string.IsNullOrEmpty(rarity))
			{
				mComboBoxOperator.ClearSelection(rarityElement);
			}
			else
			{
				mComboBoxOperator.SelectItem(rarityElement, rarity);
			}

			mTextBoxOperator.SetText(elementMap.Get(AutomationIds.ShikigamiRegisterForm.NAME), getValue(inputData, "Name"));

			mTextBoxOperator.SetText(elementMap.Get(AutomationIds.ShikigamiRegisterForm.ATTACK), getValue(inputData, "Attack"));

			mTextBoxOperator.SetText(elementMap.Get(AutomationIds.ShikigamiRegisterForm.HP), getValue(inputData, "HP"));

			mTextBoxOperator.SetText(elementMap.Get(AutomationIds.ShikigamiRegisterForm.DEFENSE), getValue(inputData, "Defense"));

			mTextBoxOperator.SetText(elementMap.Get(AutomationIds.ShikigamiRegisterForm.SPEED), getValue(inputData, "Speed"));

			mTextBoxOperator.SetText(elementMap.Get(AutomationIds.ShikigamiRegisterForm.CRIT_RATE), getValue(inputData, "CritRate"));

			mTextBoxOperator.SetText(elementMap.Get(AutomationIds.ShikigamiRegisterForm.CRIT_DAMAGE), getValue(inputData, "CritDamage"));

			mTextBoxOperator.SetText(elementMap.Get(AutomationIds.ShikigamiRegisterForm.EFFECT_HIT), getValue(inputData, "EffectHit"));

			mTextBoxOperator.SetText(elementMap.Get(AutomationIds.ShikigamiRegisterForm.EFFECT_RESIST), getValue(inputData, "EffectResist"));
		}

		private string getValue(IDictionary<string, string> inputData, string field)
		{
			return inputData[field];
		}
	}
}
