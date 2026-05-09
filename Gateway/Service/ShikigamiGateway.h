#pragma once

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Runtime::InteropServices;

#include<string>
#include "../Dto/Model/ShikigamiDto.h"
#include "../Dto/Outcome/ShikigamiDataOutcomeDto.h"

public ref class ShikigamiGateway
{
public:
	static ShikigamiDataOutcomeDto GetShikigamiList(String^ filePath, [OutAttribute] List<ShikigamiDto^>^% outShikigamiList);
	static ShikigamiDataOutcomeDto AddShikigami(String^ filePath, ShikigamiDto^ dto);
	static ShikigamiDataOutcomeDto UpdateShikigami(String^ filePath, ShikigamiDto^ oldDto, ShikigamiDto^ newDto);
};