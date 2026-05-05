#pragma once

#include "../../Core/Model/StatValue.h"
#include "../Dto/StatValueDto.h"

class StatValueConverter
{
public:
	static StatValue toNative(StatValueDto^ dto);
	static StatValueDto^ toDto(const StatValue& native);
};