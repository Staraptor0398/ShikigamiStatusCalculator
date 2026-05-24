#include "pch.h"
#include "MitamaConverter.h"

#include "StatValueConverter.h"

using namespace System::Collections::Generic;

Mitama MitamaConverter::toNative(MitamaDto^ dto)
{
	Mitama native;

	if (dto == nullptr) {
		return native;
	}

	native.MainStat = StatValueConverter::toNative(dto->MainStat);

	for (int i = 0;i < 4; i++) {
		if (dto->SubStat != nullptr && i < dto->SubStat->Count) {
			native.SubStat[i] = StatValueConverter::toNative(dto->SubStat[i]);
		}
	}

	return native;
}

MitamaDto^ MitamaConverter::toDto(const Mitama& native)
{
	MitamaDto^ dto = gcnew MitamaDto();

	dto->MainStat = StatValueConverter::toDto(native.MainStat);

	dto->SubStat = gcnew List<StatValueDto^>();

	for (int i = 0;i < 4; i++) {
		dto->SubStat->Add(StatValueConverter::toDto(native.SubStat[i]));
	}

	return dto;
}
