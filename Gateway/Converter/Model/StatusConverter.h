#pragma once

#include "../../../Core/Model/Status.h"
#include "../../Dto/Model/StatusDto.h"

// Status と StatusDto の相互変換を行うクラス
class StatusConverter
{
public:
	static Status toNative(StatusDto^ dto);
	static StatusDto^ toDto(const Status& native);
};
