#pragma once

#include "../Model/Status.h"
#include "StatusComparisonResult.h"

class StatusComparator
{
public:
	static StatusComparisonResult compare(const Status& baseStatus, const Status& targetStatus);
};
