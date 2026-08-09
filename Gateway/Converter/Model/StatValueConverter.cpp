#include "pch.h"
#include "StatValueConverter.h"

#include "StatTypeConverter.h"

StatValue StatValueConverter::toNative(StatValueDto^ dto)
{
	if (dto == nullptr) {
		throw gcnew System::ArgumentNullException("dto", "StatValueDto must not be null.");
	}

	StatValue native;

	native.Type = StatTypeConverter::toNative(dto->Type);
	native.Value = dto->Value;

	return native;
}

StatValueDto^ StatValueConverter::toDto(const StatValue& native)
{
	StatValueDto^ dto = gcnew StatValueDto();

	dto->Type = StatTypeConverter::toDto(native.Type);
	dto->Value = native.Value;

	return dto;
}
