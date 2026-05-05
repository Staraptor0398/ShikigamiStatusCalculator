#pragma once

using namespace System;
using namespace System::Collections::Generic;

#include<string>
#include "../Dto/ShikigamiDto.h"

public ref class ShikigamiGateway
{
public:
	static List<ShikigamiDto^>^ GetShikigamiList(String^ filePath);
	static void AddShikigami(String^ filePath, ShikigamiDto^ dto);
	static void UpdateShikigami(String^ filePath, ShikigamiDto^ oldDto, ShikigamiDto^ newDto);
};