#include "pch.h"
#include "CalculationGateway.h"

#include "../../Core/Service/CalculationService.h"

#include "../Converter/StatusConverter.h"
#include "../Converter/MitamaSetConverter.h"
#include "../Converter/CalculationResultConverter.h"

CalculationResultDto^ CalculationGateway::calclutate(StatusDto^ baseStatusDto, MitamaSetDto^ mitamaSetDto)
{
	Status baseStatus = StatusConverter::toNative(baseStatusDto);

	MitamaSet mitamaSet = MitamaSetConverter::toNative(mitamaSetDto);

	CalculationResult result = CalculationService::calculate(baseStatus, mitamaSet);

	return CalculationResultConverter::toDto(result);
}