#include "pch.h"
#include "CalculationResultConverter.h"

#include "StatusConverter.h"

CalculationResultDto^ CalculationResultConverter::toDto(const CalculationResult& native)
{
	CalculationResultDto^ dto = gcnew CalculationResultDto();

	dto->MitamaOnlyStatus = StatusConverter::toDto(native.mitamaOnlyStatus);

	dto->FinalStatus = StatusConverter::toDto(native.finalStatus);

	return dto;
}