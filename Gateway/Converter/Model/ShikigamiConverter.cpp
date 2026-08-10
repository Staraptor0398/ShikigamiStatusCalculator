#include "pch.h"
#include "ShikigamiConverter.h"

#include "StatusConverter.h"
#include "StringConverter.h"

Shikigami ShikigamiConverter::toNative(ShikigamiDto^ dto)
{
	if (dto == nullptr) {
		throw gcnew System::ArgumentNullException("dto", "ShikigamiDto must not be null.");
	}

	Shikigami native;

	native.Rarity = StringConverter::toUtf8String(dto->Rarity);
	native.Name = StringConverter::toUtf8String(dto->Name);
	native.Status = StatusConverter::toNative(dto->Status);

	return native;
}

ShikigamiDto^ ShikigamiConverter::toDto(Shikigami native)
{
	ShikigamiDto^ dto = gcnew ShikigamiDto();

	dto->Rarity = StringConverter::toManagedString(native.Rarity.toString());
	dto->Name = StringConverter::toManagedString(native.Name);
	dto->Status = StatusConverter::toDto(native.Status);

	return dto;
}
