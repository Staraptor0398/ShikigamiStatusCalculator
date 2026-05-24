#pragma once

#include "../Model/Status.h"
#include "../Status/StatusComparisonResult.h"

class ComparisonService
{
public:
	static StatusComparisonResult CompareStatus(const Status& baseStatus, const Status& targetStatus);
};
