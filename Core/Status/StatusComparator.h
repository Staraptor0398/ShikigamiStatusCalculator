#pragma once

#include "../Model/Status.h"
#include "StatusComparisonResult.h"

class StatusComparator
{
public:
	static StatusComparisonResult Compare(const Status& baseStatus, const Status& targetStatus);
};

