#pragma once

#include <nlohmann/json.hpp>

#include "../../Model/StatusCalculator/StatusCalculatorInputTestData.h"

class StatusCalculatorInputConverter
{
public:
	static StatusCalculatorInputTestData toTestData(const nlohmann::json& json);
};

