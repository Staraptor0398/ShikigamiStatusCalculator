#pragma once

#include <nlohmann/json.hpp>

#include "../../../Core/Model/Status.h"
#include "../Model/StatusTestData.h"

class StatusConverter
{
public:
	static StatusTestData toTestData(const nlohmann::json& json);
	static Status toNative(const StatusTestData& testData);
};
