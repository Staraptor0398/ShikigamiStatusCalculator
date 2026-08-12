#include "pch.h"
#include "StatusCalculatorInputConverter.h"

#include "../FullStatusConverter.h"
#include "../StatusConverter.h"

StatusCalculatorInputTestData StatusCalculatorInputConverter::toTestData(const nlohmann::json& json)
{
	StatusCalculatorInputTestData data{};

	data.BaseStatus = StatusConverter::toTestData(json.at("BaseStatus"));
	data.MitamaStatus = FullStatusConverter::toTestData(json.at("MitamaStatus"));

	return data;
}
