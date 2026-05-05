#pragma once

#include "Mitama.h"
#include "MitamaSet.h"
#include "../Model/Status.h"

class MitamaCalculator
{
public:
	static Status calculateSingle(const Mitama& mitama);
	static Status calculateMitamas(const Mitama mitamas[6]);
	static Status calclateSet(const MitamaSet& mitamaSet);
};