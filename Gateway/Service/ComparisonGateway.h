#pragma once

#include "../Dto/Model/StatusComparisonResultDto.h"
#include "../Dto/Model/StatusDto.h"

// Gui ⇔ Core 間のステータス比較データ受け渡しを行う Gateway クラス
public ref class ComparisonGateway
{
public:
	// ステータス比較を行う
	static StatusComparisonResultDto^ CompareStatus(StatusDto^ baseStatus, StatusDto^ targetStatus);
};
