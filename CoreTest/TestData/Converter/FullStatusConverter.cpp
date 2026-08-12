#include "pch.h"
#include "FullStatusConverter.h"

#include "StatusConverter.h"

FullStatusTestData FullStatusConverter::toTestData(const nlohmann::json& json)
{
	FullStatusTestData data{};

	static_cast<StatusTestData&>(data) = StatusConverter::toTestData(json);

	data.AdditionalAttackRate = json.at("AdditionalAttackRate").get<double>();
	data.AdditionalHpRate = json.at("AdditionalHpRate").get<double>();
	data.AdditionalDefenseRate = json.at("AdditionalDefenseRate").get<double>();

	return data;
}

Status FullStatusConverter::toNative(const FullStatusTestData& testData)
{
	Status status{};

	status = StatusConverter::toNative(testData);

	status.AdditionalAttackRate = testData.AdditionalAttackRate;
	status.AdditionalHpRate = testData.AdditionalHpRate;
	status.AdditionalDefenseRate = testData.AdditionalDefenseRate;

	return status;
}
