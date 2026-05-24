#pragma once

#include "../Model/Status.h"

class StatusCalculator
{
public:
	static Status calculateFinalStatus(const Status& baseStatus, const Status& mitamaStatus);
};
