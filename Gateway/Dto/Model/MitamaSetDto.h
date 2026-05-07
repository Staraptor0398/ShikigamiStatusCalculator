#pragma once

#include "MitamaDto.h"
#include "StatusEffectDto.h"

using namespace System::Collections::Generic;

public ref class MitamaSetDto
{
public:
	property List<MitamaDto^>^ Mitamas;
	property List<StatusEffectDto^>^ SetEffects;
	property List<StatusEffectDto^>^ UniqueEffects;
};