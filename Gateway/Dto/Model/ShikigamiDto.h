#pragma once

using namespace System;

#include "StatusDto.h"

// 式神Dtoクラス
//
// Gui ⇔ Gateway 間のデータ受け渡しで使用する。
// Core の Shikigami と対応するDto定義。
public ref class ShikigamiDto
{
public:
	String^ Name;
	String^ Rarity;

	StatusDto^ Status;

	virtual System::String^ ToString() override {
		return Name;
	}
};
