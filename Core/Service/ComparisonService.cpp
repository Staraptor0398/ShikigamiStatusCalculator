#include "pch.h"
#include "ComparisonService.h"

#include "../Status/StatusComparator.h"

StatusComparisonResult ComparisonService::compareStatus(const Status& baseStatus, const Status& targetStatus)
{
	return StatusComparator::Compare(baseStatus, targetStatus);
}
