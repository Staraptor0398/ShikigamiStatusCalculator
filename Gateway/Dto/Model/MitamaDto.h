#pragma once

#include "StatValueDto.h"

using namespace System::Collections::Generic;

// 御魂1個分のDtoクラス
//
// Gui ⇔ Gateway 間のデータ受け渡しで使用する。
// Core の StatValue と対応するDto定義。
public ref class MitamaDto
{
public:
	property StatValueDto^ MainStat;			// メインステータス

	property List<StatValueDto^>^ SubStat;		// サブステータス
};
