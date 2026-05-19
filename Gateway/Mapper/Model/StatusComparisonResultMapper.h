#pragma once

#include "../../../Core/Status/StatusComparisonResult.h"
#include "../../Dto/Model/StatusComparisonResultDto.h"

public ref class StatusComparisonResultMapper
{
public:
	static StatusComparisonResultDto^ toDto(const StatusComparisonResult& native);
};

