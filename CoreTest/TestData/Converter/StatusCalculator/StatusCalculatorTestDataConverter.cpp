#include "pch.h"
#include "StatusCalculatorTestDataConverter.h"

#include "../StatusConverter.h"
#include "StatusCalculatorInputConverter.h"

StatusCalculatorTestData StatusCalculatorTestDataConverter::toTestData(const nlohmann::json& json)
{
	StatusCalculatorTestData data{};

	data.Input = StatusCalculatorInputConverter::toTestData(json.at("Input"));
	data.Expected = StatusConverter::toTestData(json.at("Expected"));

	return data;
}
