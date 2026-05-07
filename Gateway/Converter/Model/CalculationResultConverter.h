#pragma once

#include "../../../Core/Model/CalculationResult.h"
#include "../../Dto/Model/CalculationResultDto.h"

class CalculationResultConverter
{
public:
	static CalculationResultDto^ toDto(const CalculationResult& native);
};