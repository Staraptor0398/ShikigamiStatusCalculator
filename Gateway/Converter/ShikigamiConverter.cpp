#include "pch.h"
#include "ShikigamiConverter.h"

#include "StringConverter.h"

Shikigami ShikigamiConverter::toNative(ShikigamiDto^ dto)
{
	Shikigami native;

	if (dto == nullptr) {
		return native;
	}

	native.Rarity				= StringConverter::toUtf8String(dto->Rarity);
	native.Name					= StringConverter::toUtf8String(dto->Name);
	native.Attack				= dto->Attack;
	native.HP					= dto->HP;
	native.Defense				= dto->Defense;
	native.Speed				= dto->Speed;
	native.CritRate				= dto->CritRate;
	native.CritDamage			= dto->CritDamage;
	native.EffectHit			= dto->EffectHit;
	native.EffectResist			= dto->EffectResist;

	return native;
}

ShikigamiDto^ ShikigamiConverter::toDto(Shikigami native)
{
	ShikigamiDto^ dto = gcnew ShikigamiDto();

	dto->Rarity					= StringConverter::toManagedString(native.Rarity);
	dto->Name					= StringConverter::toManagedString(native.Name);
	dto->Attack					= native.Attack;
	dto->HP						= native.HP;
	dto->Defense				= native.Defense;
	dto->Speed					= native.Speed;
	dto->CritRate				= native.CritRate;
	dto->CritDamage				= native.CritDamage;
	dto->EffectHit				= native.EffectHit;
	dto->EffectResist			= native.EffectResist;

	return dto;
}