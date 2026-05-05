#pragma once

#include "../../Core/Model/StatType.h"
#include "../Dto/StatTypeDto.h"

class StatTypeConverter
{
public:
	static StatType toNative(StatTypeDto dto);
	static StatTypeDto toDto(StatType native);
};