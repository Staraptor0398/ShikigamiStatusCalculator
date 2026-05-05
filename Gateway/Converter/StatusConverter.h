#pragma once

#include "../../Core/Model/Status.h"
#include "../Dto/StatusDto.h"

class StatusConverter
{
public:
	static Status toNative(StatusDto^ dto);
	static StatusDto^ toDto(const Status& native);
};