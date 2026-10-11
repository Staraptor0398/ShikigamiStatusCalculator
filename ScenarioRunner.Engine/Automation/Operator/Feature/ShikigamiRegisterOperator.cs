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

			string stage = "LoadInputData";
			string field = null;

			try
			{
				IDictionary<string, string> inputData = loadInputData(resolvedFilePath);

				stage = "GetRegisterForm";

				Window registerForm = getRegisterForm(session);

				stage = "GetRegisterControls";

				var elementMap = new AutomationElementMap(registerForm);

				stage = "ApplyInputData";

				applyInputData(elementMap, inputData, ref field);

				stage = "SubmitRegistration";
				field = null;

				AutomationElement registerButton = elementMap.Get(AutomationIds.ShikigamiRegisterForm.REGISTER, ControlType.Button);

				mButtonOperator.PostClick(registerButton);
			}
			catch (Exception ex)
			{
				string fieldInfo = field == null ? "" : $"Field={field}, ";

				throw new InvalidOperationException($"ShikigamiRegisterOperator.Register failed at stage '{stage}': " + $"{fieldInfo}FilePath={resolvedFilePath}. " + $"{ex.Message}", ex);
			}
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

		private void applyInputData(AutomationElementMap elementMap, IDictionary<string, string> inputData, ref string field)
		{
			field = "Rarity";

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

			field = "Name";
			mTextBoxOperator.SetText(elementMap.Get(AutomationIds.ShikigamiRegisterForm.NAME), getValue(inputData, "Name"));

			field = "Attack";
			mTextBoxOperator.SetText(elementMap.Get(AutomationIds.ShikigamiRegisterForm.ATTACK), getValue(inputData, "Attack"));

			field = "HP";
			mTextBoxOperator.SetText(elementMap.Get(AutomationIds.ShikigamiRegisterForm.HP), getValue(inputData, "HP"));

			field = "Defense";
			mTextBoxOperator.SetText(elementMap.Get(AutomationIds.ShikigamiRegisterForm.DEFENSE), getValue(inputData, "Defense"));

			field = "Speed";
			mTextBoxOperator.SetText(elementMap.Get(AutomationIds.ShikigamiRegisterForm.SPEED), getValue(inputData, "Speed"));

			field = "CritRate";
			mTextBoxOperator.SetText(elementMap.Get(AutomationIds.ShikigamiRegisterForm.CRIT_RATE), getValue(inputData, "CritRate"));

			field = "CritDamage";
			mTextBoxOperator.SetText(elementMap.Get(AutomationIds.ShikigamiRegisterForm.CRIT_DAMAGE), getValue(inputData, "CritDamage"));

			field = "EffectHit";
			mTextBoxOperator.SetText(elementMap.Get(AutomationIds.ShikigamiRegisterForm.EFFECT_HIT), getValue(inputData, "EffectHit"));

			field = "EffectResist";
			mTextBoxOperator.SetText(elementMap.Get(AutomationIds.ShikigamiRegisterForm.EFFECT_RESIST), getValue(inputData, "EffectResist"));
		}

		private string getValue(IDictionary<string, string> inputData, string field)
		{
			return inputData[field];
		}
	}
}
