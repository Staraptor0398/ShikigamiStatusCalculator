#include "pch.h"
#include "ComparisonService.h"

#include "../Status/StatusComparator.h"

StatusComparisonResult ComparisonService::CompareStatus(const Status& baseStatus, const Status& targetStatus)
{
	return StatusComparator::compare(baseStatus, targetStatus);
}
