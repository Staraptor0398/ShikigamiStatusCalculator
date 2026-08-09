#include "pch.h"
#include "MitamaSetConverter.h"

#include "MitamaConverter.h"
#include "StatValueConverter.h"

using namespace System::Collections::Generic;

MitamaSet MitamaSetConverter::toNative(MitamaSetDto^ dto)
{
	if (dto == nullptr) {
		throw gcnew System::ArgumentNullException("dto", "MitamaSetDto must not be null.");
	}

	MitamaSet native;

	auto mitamas = dto->Mitamas;
	auto setEffects = dto->SetEffects;
	auto uniqueEffects = dto->UniqueEffects;

	for (int i = 0; i < 6; i++) {
		if (mitamas != nullptr && i < mitamas->Count) {
			native.Mitamas[i] = MitamaConverter::toNative(mitamas->default[i]);
		}
	}

	for (int i = 0; i < 3; i++) {
		if (setEffects != nullptr && i < setEffects->Count) {
			if (setEffects->default[i] == nullptr) {
				throw gcnew System::ArgumentNullException("dto", "SetEffectDto must not be null.");
			}

			native.SetEffects[i].Stat = StatValueConverter::toNative(setEffects->default[i]->Stat);
		}
	}

	for (int i = 0; i < 6; i++) {
		if (uniqueEffects != nullptr && i < uniqueEffects->Count) {
			if (uniqueEffects->default[i] == nullptr) {
				throw gcnew System::ArgumentNullException("dto", "SetEffectDto must not be null.");
			}

			native.UniqueEffects[i].Stat = StatValueConverter::toNative(uniqueEffects->default[i]->Stat);
		}
	}

	return native;
}

MitamaSetDto^ MitamaSetConverter::toDto(const MitamaSet& native)
{
	MitamaSetDto^ dto = gcnew MitamaSetDto();

	dto->Mitamas = gcnew List<MitamaDto^>();
	dto->SetEffects = gcnew List<SetEffectDto^>();
	dto->UniqueEffects = gcnew List<SetEffectDto^>();

	for (int i = 0; i < 6; i++) {
		dto->Mitamas->Add(MitamaConverter::toDto(native.Mitamas[i]));
	}

	for (int i = 0; i < 3; i++) {
		SetEffectDto^ effect = gcnew SetEffectDto();

		effect->Stat = StatValueConverter::toDto(native.SetEffects[i].Stat);

		dto->SetEffects->Add(effect);
	}

	for (int i = 0; i < 6; i++) {
		SetEffectDto^ effect = gcnew SetEffectDto();

		effect->Stat = StatValueConverter::toDto(native.UniqueEffects[i].Stat);

		dto->UniqueEffects->Add(effect);
	}

	return dto;
}
