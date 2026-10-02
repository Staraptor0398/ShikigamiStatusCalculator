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
	public class ShikigamiEditOperator
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

		private readonly WindowWaiter mWindowWaiter;
		private readonly ProcessWindowWaiter mProcessWindowWaiter;

		private GuiSession mEditSession;
		private Window mEditForm;

		public ShikigamiEditOperator()
		{
			mButtonOperator = new ButtonOperator();
			mComboBoxOperator = new ComboBoxOperator();
			mTextBoxOperator = new TextBoxOperator();
			mGuiOperator = new GuiOperator();

			mWindowWaiter = new WindowWaiter();
			mProcessWindowWaiter = new ProcessWindowWaiter();
		}

		public void Open(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			resetEditFormCache();

			AutomationElement button = mGuiOperator.GetMainElement(session, AutomationIds.MainForm.EDIT_SHIKIGAMI, ControlType.Button);

			mButtonOperator.PostClick(button);
		}

		public void Edit(ScenarioExecutonContext context, string filePath)
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

			Window editForm = getEditForm(session);
			var elementMap = new AutomationElementMap(editForm);

			applyInputData(elementMap, inputData);

			AutomationElement updateButton = elementMap.Get(AutomationIds.ShikigamiRegisterForm.REGISTER, ControlType.Button);

			mButtonOperator.PostClick(updateButton);

			resetEditFormCache();
		}

		public void Save(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			Window editForm = getEditForm(session);

			mButtonOperator.Click(editForm, AutomationIds.ShikigamiRegisterForm.REGISTER);

			mWindowWaiter.WaitForWindowClosed(editForm);

			resetEditFormCache();

			if (!session.Application.WaitWhileBusy(TimeSpan.FromSeconds(5)))
			{
				throw new InvalidOperationException("Shikigami update did not complete within 5 seconds.");
			}
		}

		private Window getEditForm(GuiSession session)
		{
			if (ReferenceEquals(mEditSession, session) && mEditForm != null)
			{
				return mEditForm;
			}

			Window mainWindow = mGuiOperator.GetMainWindow(session);

			mEditForm = mProcessWindowWaiter.WaitForProcessWindow(session, mainWindow, element => element.Properties.AutomationId.ValueOrDefault == AutomationIds.ShikigamiRegisterForm.ID);

			mEditSession = session;

			return mEditForm;
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

		private string getValue(
			IDictionary<string, string> inputData,
			string field)
		{
			return inputData[field];
		}

		private void resetEditFormCache()
		{
			mEditForm = null;
			mEditSession = null;
		}
	}
}
