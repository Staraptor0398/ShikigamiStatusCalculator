#include "pch.h"
#include "CalculationService.h"
#include "../Mitama/MitamaCalculator.h"
#include "../Status/StatusCalculator.h"

CalculationResult CalculationService::calculate(const Status& baseStatus, const MitamaSet& mitamaSet)
{
	CalculationResult result;

	result.mitamaOnlyStatus = MitamaCalculator::calclateSet(mitamaSet);
	result.finalStatus = StatusCalculator::calculateFinalStatus(baseStatus, result.mitamaOnlyStatus);

	return result;
}
