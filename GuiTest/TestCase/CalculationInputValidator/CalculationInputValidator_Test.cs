using Gui.Common;
using Gui.Converter;
using Gui.Model;
using Gui.Validation;
using GuiTest.TestCommon;
using SaveData.Model;

namespace GuiTest.TestCase
{
	[TestClass]
	public class CalculationInputValidator_Test
	{
		private MitamaSetInputModel loadValidInputModel()
		{
			MitamaSetSaveData saveData = TestDataLoader.LoadValidMitamaSet();
			return MitamaSetConverter.ToInputModel(saveData);
		}

		[TestMethod]
		public void Success()
		{
			MitamaSetInputModel inputModel = loadValidInputModel();

			CalculationInputValidationOutcome actual = CalculationInputValidator.Validate(inputModel);

			Assert.AreEqual(CalculationInputValidationOutcome.SUCCESS, actual);
		}

		[TestMethod]
		public void NoEquippedMitama()
		{
			MitamaSetInputModel inputModel = new MitamaSetInputModel();

			CalculationInputValidationOutcome actual = CalculationInputValidator.Validate(inputModel);

			Assert.AreEqual(CalculationInputValidationOutcome.NO_EQUIPPED_MITAMA, actual);
		}

		[TestMethod]
		public void MainNotSelectedWithSubStat()
		{
			MitamaSetInputModel inputModel = loadValidInputModel();

			inputModel.Mitamas[0].MainStat.Type = "";
			inputModel.Mitamas[0].MainStat.ValueText = "";

			CalculationInputValidationOutcome actual = CalculationInputValidator.Validate(inputModel);

			Assert.AreEqual(CalculationInputValidationOutcome.MAIN_STAT_NOT_SELECTED_WITH_SUB_STAT, actual);
		}

		[TestMethod]
		public void SubStatTypeWithoutValue()
		{
			MitamaSetInputModel inputModel = loadValidInputModel();

			inputModel.Mitamas[0].SubStat[0].ValueText = "";

			CalculationInputValidationOutcome actual = CalculationInputValidator.Validate(inputModel);

			Assert.AreEqual(CalculationInputValidationOutcome.SUB_STAT_TYPE_WITHOUT_VALUE, actual);
		}

		[TestMethod]
		public void SubStatValueWithoutType()
		{
			MitamaSetInputModel inputModel = loadValidInputModel();

			inputModel.Mitamas[0].SubStat[0].Type = DisplayText.NONE;

			CalculationInputValidationOutcome actual = CalculationInputValidator.Validate(inputModel);

			Assert.AreEqual(CalculationInputValidationOutcome.SUB_STAT_VALUE_WITHOUT_TYPE, actual);
		}

		[TestMethod]
		public void InvalidValue()
		{
			MitamaSetInputModel inputModel = loadValidInputModel();

			inputModel.Mitamas[0].SubStat[0].ValueText = "abc";

			CalculationInputValidationOutcome actual = CalculationInputValidator.Validate(inputModel);

			Assert.AreEqual(CalculationInputValidationOutcome.INVALID_VALUE, actual);
		}


		[TestMethod]
		public void NegativeValue()
		{
			MitamaSetInputModel inputModel = loadValidInputModel();

			inputModel.Mitamas[0].SubStat[0].ValueText = "-1";

			CalculationInputValidationOutcome actual = CalculationInputValidator.Validate(inputModel);

			Assert.AreEqual(CalculationInputValidationOutcome.NEGATIVE_VALUE, actual);
		}

		[TestMethod]
		public void DuplicateSubStat()
		{
			MitamaSetInputModel inputModel = loadValidInputModel();

			inputModel.Mitamas[0].SubStat[0].Type = DisplayText.ATTACK;
			inputModel.Mitamas[0].SubStat[1].Type = DisplayText.ATTACK;

			CalculationInputValidationOutcome actual = CalculationInputValidator.Validate(inputModel);

			Assert.AreEqual(CalculationInputValidationOutcome.DUPLICATE_SUB_STAT, actual);
		}

		[TestMethod]
		public void EffectSlotCountExceedsEquippedSlots()
		{
			MitamaSetInputModel inputModel = loadValidInputModel();

			inputModel.SetEffects[0].Stat.Type = DisplayText.ADDITIONAL_ATTACK_RATE;
			inputModel.SetEffects[1].Stat.Type = DisplayText.ADDITIONAL_ATTACK_RATE;
			inputModel.SetEffects[2].Stat.Type = DisplayText.ADDITIONAL_ATTACK_RATE;
			inputModel.UniqueEffects[0].Stat.Type = DisplayText.ADDITIONAL_ATTACK_RATE;

			CalculationInputValidationOutcome actual = CalculationInputValidator.Validate(inputModel);

			Assert.AreEqual(CalculationInputValidationOutcome.EFFECT_SLOT_COUNT_EXCEEDS_EQUIPPED_SLOTS, actual);
		}
	}
}
