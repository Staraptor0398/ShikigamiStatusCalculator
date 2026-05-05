#include "pch.h"
#include "StatValueConverter.h"

#include "StatTypeConverter.h"

StatValue StatValueConverter::toNative(StatValueDto^ dto)
{
	StatValue native;

	if (dto == nullptr) {
		native.Type = StatType::None;
		native.Value = 0.0;
		return native;
	}

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
