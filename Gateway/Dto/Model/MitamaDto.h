#pragma once

#include "StatValueDto.h"

using namespace System::Collections::Generic;

public ref class MitamaDto
{
public:
	property StatValueDto^ MainStat;

	property List<StatValueDto^>^ SubStat;
};