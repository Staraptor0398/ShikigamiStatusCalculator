#include "pch.h"
#include "ComparisonGateway.h"

#include "../../Core/Service/ComparisonService.h"

#include "../Converter/Model/StatusConverter.h"
#include "../Mapper/Model/StatusComparisonResultMapper.h"

StatusComparisonResultDto^ ComparisonGateway::compareStatus(StatusDto^ baseStatus, StatusDto^ targetStatus)
{
	Status baseNative = StatusConverter::toNative(baseStatus);

	Status targetNative = StatusConverter::toNative(targetStatus);

	StatusComparisonResult result = ComparisonService::compareStatus(baseNative, targetNative);

	return StatusComparisonResultMapper::toDto(result);
}
