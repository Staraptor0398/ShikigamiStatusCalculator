#pragma once

#include "../../../Core/Status/StatusComparisonResult.h"
#include "../../Dto/Model/StatusComparisonResultDto.h"

// StatusComparisonResult から StatusComparisonResultDto への片方向変換を行うクラス
public ref class StatusComparisonResultMapper
{
public:
	static StatusComparisonResultDto^ toDto(const StatusComparisonResult& native);
};
