#pragma once

#include <nlohmann/json.hpp>

#include "../../../../Core/Mitama/MitamaSet.h"
#include "../../Model/FullStatusTestData.h"
#include "../../Model/TestData.h"

class MitamaCalculatorTestDataConverter
{
public:
	static TestData<MitamaSet, FullStatusTestData> toTestData(const nlohmann::json& json);
};
