#pragma once

#include <nlohmann/json.hpp>

#include "../../Model/StatusCalculator/StatusCalculatorTestData.h"

class StatusCalculatorTestDataConverter {
public:
	static StatusCalculatorTestData toTestData(const nlohmann::json& json);
};
