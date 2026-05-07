#include "pch.h"
#include "MitamaSetConverter.h"

#include "MitamaConverter.h"
#include "StatValueConverter.h"

using namespace System::Collections::Generic;

MitamaSet MitamaSetConverter::toNative(MitamaSetDto^ dto)
{
	MitamaSet native;

	if (dto == nullptr) {
		return native;
	}

	auto mitamas = dto->Mitamas;
	auto setEffects = dto->SetEffects;
	auto uniqueEffects = dto->UniqueEffects;

	for (int i = 0; i < 6; i++) {
		if (dto->Mitamas != nullptr && i < dto->Mitamas->Count) {
			native.Mitamas[i] = MitamaConverter::toNative(mitamas->default[i]);
		}
	}

	for (int i = 0; i < 3; i++) {
		if (dto->SetEffects != nullptr && i < dto->SetEffects->Count) {
			native.SetEffects[i].Stat = StatValueConverter::toNative(setEffects->default[i]->Stat);
		}
	}

	for (int i = 0; i < 6; i++) {
		if (dto->UniqueEffects != nullptr && i < dto->UniqueEffects->Count) {
			native.UniqueEffects[i].Stat = StatValueConverter::toNative(uniqueEffects->default[i]->Stat);
		}
	}

	return native;
}

MitamaSetDto^ MitamaSetConverter::toDto(const MitamaSet& native)
{
	MitamaSetDto^ dto = gcnew MitamaSetDto();

	dto->Mitamas = gcnew List<MitamaDto^>();
	dto->SetEffects = gcnew List<StatusEffectDto^>();
	dto->UniqueEffects = gcnew List<StatusEffectDto^>();

	for (int i = 0; i < 6; i++) {
		dto->Mitamas->Add(MitamaConverter::toDto(native.Mitamas[i]));
	}

	for (int i = 0; i < 3; i++) {
		StatusEffectDto^ effect = gcnew StatusEffectDto();

		effect->Stat = StatValueConverter::toDto(native.SetEffects[i].Stat);

		dto->SetEffects->Add(effect);
	}

	for (int i = 0; i < 6; i++) {
		StatusEffectDto^ effect = gcnew StatusEffectDto();

		effect->Stat = StatValueConverter::toDto(native.UniqueEffects[i].Stat);

		dto->UniqueEffects->Add(effect);
	}
	
	return dto;
}