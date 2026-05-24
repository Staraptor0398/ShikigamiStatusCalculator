#pragma once

#include "../Dto/Model/CalculationResultDto.h"
#include "../Dto/Model/MitamaSetDto.h"
#include "../Dto/Model/StatusDto.h"

// Gui ⇔ Core 間の計算データ受け渡しを行う Gateway クラス
public ref class CalculationGateway
{
public:
	// ステータス計算を行う
	static CalculationResultDto^ Calclutate(StatusDto^ baseStatusDto, MitamaSetDto^ mitamaSetDto);
};
