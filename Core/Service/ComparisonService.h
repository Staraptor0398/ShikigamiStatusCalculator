#pragma once

#include "../Model/Status.h"
#include "../Status/StatusComparisonResult.h"

class ComparisonService
{
public:
	static StatusComparisonResult compareStatus(const Status& baseStatus, const Status& targetStatus);
};
