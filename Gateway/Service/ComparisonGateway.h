#pragma once

#include "../Dto/Model/StatusComparisonResultDto.h"
#include "../Dto/Model/StatusDto.h"

public ref class ComparisonGateway
{
public:
	static StatusComparisonResultDto^ compareStatus(StatusDto^ baseStatus, StatusDto^ targetStatus);
};
