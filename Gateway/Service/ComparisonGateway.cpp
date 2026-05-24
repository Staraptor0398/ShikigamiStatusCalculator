#include "pch.h"
#include "ComparisonGateway.h"

#include "../../Core/Service/ComparisonService.h"

#include "../Converter/Model/StatusConverter.h"
#include "../Mapper/Model/StatusComparisonResultMapper.h"

StatusComparisonResultDto^ ComparisonGateway::CompareStatus(StatusDto^ baseStatus, StatusDto^ targetStatus)
{
	Status baseNative = StatusConverter::toNative(baseStatus);

	Status targetNative = StatusConverter::toNative(targetStatus);

	StatusComparisonResult result = ComparisonService::CompareStatus(baseNative, targetNative);

	return StatusComparisonResultMapper::toDto(result);
}
