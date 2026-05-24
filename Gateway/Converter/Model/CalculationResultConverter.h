#pragma once

#include "../../../Core/Model/CalculationResult.h"
#include "../../Dto/Model/CalculationResultDto.h"

// CalculationResult から CalculationResultDto への片方向変換を行うクラス
class CalculationResultConverter
{
public:
	static CalculationResultDto^ toDto(const CalculationResult& native);
};
