#pragma once

#include "MitamaDto.h"
#include "SetEffectDto.h"

using namespace System::Collections::Generic;

// 御魂セットのDtoクラス
//
// Gui ⇔ Gateway 間のデータ受け渡しで使用する。
// Core の MitamaSet と対応するDto定義。
public ref class MitamaSetDto
{
public:
	property List<MitamaDto^>^ Mitamas;				// 御魂
	property List<SetEffectDto^>^ SetEffects;		// 2セット効果
	property List<SetEffectDto^>^ UniqueEffects;	// 固有効果
};
