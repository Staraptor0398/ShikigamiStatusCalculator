#include "pch.h"
#include "StatusCalculator_Test.h"

#include "../../../Core/Model/Status.h"
#include "../../../Core/Status/StatusCalculator.h"
#include "../../TestCommon/TestAssert.h"
#include "../../TestData/Access/JsonDataAccess.h"
#include "../../TestData/Converter/FullStatusConverter.h"
#include "../../TestData/Converter/StatusCalculator/StatusCalculatorTestDataConverter.h"
#include "../../TestData/Converter/StatusConverter.h"
#include "../../TestData/Model/StatusCalculator/StatusCalculatorTestData.h"
#include "../../TestData/TestDataDirectory.h"

#include <algorithm>
#include <filesystem>
#include <vector>

void StatusCalculator_Test::calculateFinalStatus_matchesJsonTestData()
{
	const std::filesystem::path dataDirectory = TestDataDirectory::getPath("T001.json").parent_path();
	std::vector<std::filesystem::path> testFiles;

	for (const auto& entry : std::filesystem::directory_iterator(dataDirectory)) {
		if (entry.is_regular_file() && entry.path().extension() == ".json") {
			testFiles.push_back(entry.path());
		}
	}

	std::sort(testFiles.begin(), testFiles.end());
	ASSERT_FALSE(testFiles.empty()) << "No JSON files found in " << dataDirectory.string();

	for (const auto& filePath : testFiles) {
		SCOPED_TRACE(filePath.filename().string());

		const auto testData = JsonDataAccess::load<StatusCalculatorTestData, StatusCalculatorTestDataConverter>(filePath);
		const Status baseStatus = StatusConverter::toNative(testData.Input.BaseStatus);
		const Status mitamaStatus = FullStatusConverter::toNative(testData.Input.MitamaStatus);
		const Status expected = StatusConverter::toNative(testData.Expected);
		const Status actual = StatusCalculator::calculateFinalStatus(baseStatus, mitamaStatus);

		TEST_ASSERT_DOUBLE_EQUAL(expected.Attack, actual.Attack);
		TEST_ASSERT_DOUBLE_EQUAL(expected.Hp, actual.Hp);
		TEST_ASSERT_DOUBLE_EQUAL(expected.Defense, actual.Defense);
		TEST_ASSERT_DOUBLE_EQUAL(expected.Speed, actual.Speed);
		TEST_ASSERT_DOUBLE_EQUAL(expected.CriticalRate, actual.CriticalRate);
		TEST_ASSERT_DOUBLE_EQUAL(expected.CriticalDamage, actual.CriticalDamage);
		TEST_ASSERT_DOUBLE_EQUAL(expected.EffectHit, actual.EffectHit);
		TEST_ASSERT_DOUBLE_EQUAL(expected.EffectResist, actual.EffectResist);
	}
}
