#pragma once

#include <nlohmann/json.hpp>

#include "../../../Core/Model/Status.h"
#include "../Model/FullStatusTestData.h"

class FullStatusConverter
{
public:
	static FullStatusTestData toTestData(const nlohmann::json& json);
	static Status toNative(const FullStatusTestData& testData);
};

