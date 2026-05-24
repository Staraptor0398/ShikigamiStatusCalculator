#pragma once

#include "../Model/Status.h"
#include "Mitama.h"
#include "MitamaSet.h"

class MitamaCalculator
{
public:
	static Status calculate(const MitamaSet& mitamaSet);
public:
	static Status calculateSingle(const Mitama& mitama);
	static Status calculateMitamas(const Mitama mitamas[6]);
	static void applyStat(Status& status, const StatValue& stat);
};
