#include "pch.h"
#include "../Mitama/MitamaCalculator.h"
#include "../Status/StatusCalculator.h"
#include "CalculationService.h"

CalculationResult CalculationService::calculate(const Status& baseStatus, const MitamaSet& mitamaSet)
{
	CalculationResult result;

	result.mitamaOnlyStatus = MitamaCalculator::calculate(mitamaSet);
	result.finalStatus = StatusCalculator::calculateFinalStatus(baseStatus, result.mitamaOnlyStatus);

	return result;
}
