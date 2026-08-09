#include "pch.h"
#include "StatTypeConverter.h"

StatType StatTypeConverter::toNative(StatTypeDto dto)
{
	switch (dto) {
		case StatTypeDto::None:
			return StatType::None;
		case StatTypeDto::Attack:
			return StatType::Attack;
		case StatTypeDto::Hp:
			return StatType::Hp;
		case StatTypeDto::Defense:
			return StatType::Defense;
		case StatTypeDto::Speed:
			return StatType::Speed;
		case StatTypeDto::CriticalRate:
			return StatType::CriticalRate;
		case StatTypeDto::CriticalDamage:
			return StatType::CriticalDamage;
		case StatTypeDto::EffectHit:
			return StatType::EffectHit;
		case StatTypeDto::EffectResist:
			return StatType::EffectResist;
		case StatTypeDto::AdditionalAttackRate:
			return StatType::AdditionalAttackRate;
		case StatTypeDto::AdditionalHpRate:
			return StatType::AdditionalHpRate;
		case StatTypeDto::AdditionalDefenseRate:
			return StatType::AdditionalDefenseRate;
		default:
			throw gcnew System::ArgumentOutOfRangeException("dto", "Invalid StatTypeDto value");
	}
}

StatTypeDto StatTypeConverter::toDto(StatType native)
{
	switch (native) {
		case StatType::None:
			return StatTypeDto::None;
		case StatType::Attack:
			return StatTypeDto::Attack;
		case StatType::Hp:
			return StatTypeDto::Hp;
		case StatType::Defense:
			return StatTypeDto::Defense;
		case StatType::Speed:
			return StatTypeDto::Speed;
		case StatType::CriticalRate:
			return StatTypeDto::CriticalRate;
		case StatType::CriticalDamage:
			return StatTypeDto::CriticalDamage;
		case StatType::EffectHit:
			return StatTypeDto::EffectHit;
		case StatType::EffectResist:
			return StatTypeDto::EffectResist;
		case StatType::AdditionalAttackRate:
			return StatTypeDto::AdditionalAttackRate;
		case StatType::AdditionalHpRate:
			return StatTypeDto::AdditionalHpRate;
		case StatType::AdditionalDefenseRate:
			return StatTypeDto::AdditionalDefenseRate;
		default:
			throw gcnew System::InvalidOperationException("Invalid StatType value.");
	}
}
