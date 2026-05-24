#include "pch.h"
#include "CalculationGateway.h"

#include "../../Core/Service/CalculationService.h"

#include "../Converter/Model/CalculationResultConverter.h"
#include "../Converter/Model/MitamaSetConverter.h"
#include "../Converter/Model/StatusConverter.h"

CalculationResultDto^ CalculationGateway::Calclutate(StatusDto^ baseStatusDto, MitamaSetDto^ mitamaSetDto)
{
	Status baseStatus = StatusConverter::toNative(baseStatusDto);

	MitamaSet mitamaSet = MitamaSetConverter::toNative(mitamaSetDto);

	CalculationResult result = CalculationService::calculate(baseStatus, mitamaSet);

	return CalculationResultConverter::toDto(result);
}
