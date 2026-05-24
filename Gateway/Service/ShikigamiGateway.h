#pragma once

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Runtime::InteropServices;

#include "../Dto/Model/ShikigamiDto.h"
#include "../Dto/Outcome/ShikigamiDataOutcomeDto.h"
#include<string>

// Gui ⇔ Core 間の式神データ受け渡しを行う Gateway クラス
public ref class ShikigamiGateway
{
public:
	// 式神一覧を取得する
	static ShikigamiDataOutcomeDto GetShikigamiList(String^ filePath, [OutAttribute] List<ShikigamiDto^>^% outShikigamiList);

	// 式神データを追加する
	static ShikigamiDataOutcomeDto AddShikigami(String^ filePath, ShikigamiDto^ dto);

	// 式神データを更新する
	static ShikigamiDataOutcomeDto UpdateShikigami(String^ filePath, ShikigamiDto^ oldDto, ShikigamiDto^ newDto);
};
