#include "pch.h"
#include "StatusConverter.h"

Status StatusConverter::toNative(StatusDto^ dto)
{
	Status native;

	if (dto == nullptr) {
		return native;
	}

	native.Attack = dto->Attack;
	native.Hp = dto->HP;
	native.Defense = dto->Defense;
	native.Speed = dto->Speed;
	native.CriticalRate = dto->CritRate;
	native.CriticalDamage = dto->CritDamage;
	native.EffectHit = dto->EffectHit;
	native.EffectResist = dto->EffectResist;
	native.AdditionalAttackRate = dto->AdditionalAttackRate;
	native.AdditionalHpRate = dto->AdditionalHpRate;
	native.AdditionalDefenseRate = dto->AdditionalDefenseRate;

	return native;
}

StatusDto^ StatusConverter::toDto(const Status& native)
{
	StatusDto^ dto = gcnew StatusDto();

	dto->Attack = native.Attack;
	dto->HP = native.Hp;;
	dto->Defense = native.Defense;
	dto->Speed = native.Speed;
	dto->CritRate = native.CriticalRate;
	dto->CritDamage = native.CriticalDamage;
	dto->EffectHit = native.EffectHit;
	dto->EffectResist = native.EffectResist;
	dto->AdditionalAttackRate = native.AdditionalAttackRate;
	dto->AdditionalHpRate = native.AdditionalHpRate;
	dto->AdditionalDefenseRate = native.AdditionalDefenseRate;

	return dto;
}
