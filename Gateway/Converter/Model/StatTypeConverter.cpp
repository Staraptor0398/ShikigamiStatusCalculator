#include "pch.h"
#include "StatTypeConverter.h"

StatType StatTypeConverter::toNative(StatTypeDto dto)
{
	StatType native = StatType::None;

	switch (dto) {
		case StatTypeDto::Attack:						native = StatType::Attack;						break;
		case StatTypeDto::Hp:							native = StatType::Hp;							break;
		case StatTypeDto::Defense:						native = StatType::Defense;						break;
		case StatTypeDto::Speed:						native = StatType::Speed;						break;
		case StatTypeDto::CriticalRate:					native = StatType::CriticalRate;				break;
		case StatTypeDto::CriticalDamage:				native = StatType::CriticalDamage;				break;
		case StatTypeDto::EffectHit:					native = StatType::EffectHit;					break;
		case StatTypeDto::EffectResist:					native = StatType::EffectResist;				break;
		case StatTypeDto::AdditionalAttackRate:			native = StatType::AdditionalAttackRate;		break;
		case StatTypeDto::AdditionalHpRate:				native = StatType::AdditionalHpRate;			break;
		case StatTypeDto::AdditionalDeffenseRate:		native = StatType::AdditionalDeffenseRate;		break;
		default:																						break;
	}

	return native;
}

StatTypeDto StatTypeConverter::toDto(StatType native)
{
	StatTypeDto dto = StatTypeDto::None;

	switch (native) {
		case StatType::Attack:							dto = StatTypeDto::Attack;						break;
		case StatType::Hp:								dto = StatTypeDto::Hp;							break;
		case StatType::Defense:							dto = StatTypeDto::Defense;						break;
		case StatType::Speed:							dto = StatTypeDto::Speed;						break;
		case StatType::CriticalRate:					dto = StatTypeDto::CriticalRate;				break;
		case StatType::CriticalDamage:					dto = StatTypeDto::CriticalDamage;				break;
		case StatType::EffectHit:						dto = StatTypeDto::EffectHit;					break;
		case StatType::EffectResist:					dto = StatTypeDto::EffectResist;				break;
		case StatType::AdditionalAttackRate:			dto = StatTypeDto::AdditionalAttackRate;		break;
		case StatType::AdditionalHpRate:				dto = StatTypeDto::AdditionalHpRate;			break;
		case StatType::AdditionalDeffenseRate:			dto = StatTypeDto::AdditionalDeffenseRate;		break;
		default:																						break;
	}

	return dto;
}
