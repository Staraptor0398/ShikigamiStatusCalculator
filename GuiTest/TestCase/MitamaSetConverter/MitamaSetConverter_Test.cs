using Gui.Converter;
using Gui.Model;
using GuiTest.TestCommon;
using GuiTest.TestCommon.Converter;
using GuiTest.TestCommon.Model;
using SaveData.Model;

namespace GuiTest.TestCase
{
	[TestClass]
	public class MitamaSetConverter_Test
	{
		[TestMethod]
		public void DtoToSaveData()
		{
			MitamaSetSaveData source = TestDataLoader.LoadValidMitamaSet();
			MitamaSetInputModel inputModel = MitamaSetConverter.ToInputModel(source);
			MitamaSetDto dto = MitamaSetConverter.ToDto(inputModel);

			MitamaSetTestData expected = GuiTestDataConverter.ToTestData(dto);

			MitamaSetSaveData saveData = MitamaSetConverter.ToSaveData(dto);

			MitamaSetTestData actual = GuiTestDataConverter.ToTestData(saveData);

			TestAssert.AreEqual(expected, actual);

			for (int i = 0; i < saveData.Mitamas.Count; i++)
			{
				Assert.AreEqual(i + 1, saveData.Mitamas[i].Slot);
			}
		}

		[TestMethod]
		public void InputModelToSaveData()
		{
			MitamaSetSaveData source = TestDataLoader.LoadValidMitamaSet();
			MitamaSetInputModel inputModel = MitamaSetConverter.ToInputModel(source);

			MitamaSetTestData expected = GuiTestDataConverter.ToTestData(inputModel);

			MitamaSetSaveData saveData = MitamaSetConverter.ToSaveData(inputModel);

			MitamaSetTestData actual = GuiTestDataConverter.ToTestData(saveData);

			TestAssert.AreEqual(expected, actual);

			for (int i = 0; i < saveData.Mitamas.Count; i++)
			{
				Assert.AreEqual(i + 1, saveData.Mitamas[i].Slot);
			}
		}

		[TestMethod]
		public void InputModelToDto()
		{
			MitamaSetSaveData saveData = TestDataLoader.LoadValidMitamaSet();
			MitamaSetInputModel inputModel = MitamaSetConverter.ToInputModel(saveData);

			MitamaSetTestData expected = GuiTestDataConverter.ToTestData(inputModel);

			MitamaSetDto dto = MitamaSetConverter.ToDto(inputModel);

			MitamaSetTestData actual = GuiTestDataConverter.ToTestData(dto);

			TestAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void SaveDataToInputModel()
		{
			MitamaSetSaveData saveData = TestDataLoader.LoadValidMitamaSet();

			MitamaSetTestData expected = GuiTestDataConverter.ToTestData(saveData);

			MitamaSetInputModel inputModel = MitamaSetConverter.ToInputModel(saveData);

			MitamaSetTestData actual = GuiTestDataConverter.ToTestData(inputModel);

			TestAssert.AreEqual(expected, actual);
		}


	}
}
