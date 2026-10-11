#include "pch.h"
#include "MitamaCalculatorTestDataConverter.h"

#include "../FullStatusConverter.h"

#include <stdexcept>
#include <string>

namespace {
	StatType toStatType(const nlohmann::json& value)
	{
		int type;

		if (value.is_number_integer()) {
			type = value.get<int>();
		}
		else if (value.is_string()) {
			const std::string name = value.get<std::string>();

			if (name == "None") return StatType::None;
			if (name == "Attack") return StatType::Attack;
			if (name == "Hp") return StatType::Hp;
			if (name == "Defense") return StatType::Defense;
			if (name == "Speed") return StatType::Speed;
			if (name == "CriticalRate") return StatType::CriticalRate;
			if (name == "CriticalDamage") return StatType::CriticalDamage;
			if (name == "EffectHit") return StatType::EffectHit;
			if (name == "EffectResist") return StatType::EffectResist;
			if (name == "AdditionalAttackRate") return StatType::AdditionalAttackRate;
			if (name == "AdditionalHpRate") return StatType::AdditionalHpRate;
			if (name == "AdditionalDefenseRate") return StatType::AdditionalDefenseRate;

			throw std::invalid_argument("Unknown stat type: " + name);
		}
		else {
			throw std::invalid_argument("Invalid stat type.");
		}

		if (type < static_cast<int>(StatType::None) || type > static_cast<int>(StatType::AdditionalDefenseRate)) {
			throw std::out_of_range("Stat type is out of range.");
		}

		return static_cast<StatType>(type);
	}

	StatValue toStatValue(const nlohmann::json& json)
	{
		StatValue stat{};

		if (json.is_null()) {
			return stat;
		}

		stat.Type = toStatType(json.at("Type"));
		stat.Value = json.at("Value").get<double>();
		return stat;
	}

	MitamaSet toMitamaSet(const nlohmann::json& json)
	{
		MitamaSet result{};
		const auto& mitamas = json.at("Mitamas");
		const auto& setEffects = json.at("SetEffects");
		const auto& uniqueEffects = json.at("UniqueEffects");

		if (!mitamas.is_array() || mitamas.size() != 6 ||
			!setEffects.is_array() || setEffects.size() != 3 ||
			!uniqueEffects.is_array() || uniqueEffects.size() != 6) {
			throw std::invalid_argument("Invalid MitamaSet array size.");
		}

		for (size_t i = 0; i < 6; i++) {
			const auto& mitama = mitamas.at(i);
			const auto& subStats = mitama.at("SubStats");

			if (!subStats.is_array() || subStats.size() != 4) {
				throw std::invalid_argument("Invalid SubStats array size.");
			}

			result.Mitamas[i].MainStat = toStatValue(mitama.at("MainStat"));

			for (size_t j = 0; j < 4; j++) {
				result.Mitamas[i].SubStat[j] = toStatValue(subStats.at(j));
			}
		}

		for (size_t i = 0; i < 3; i++) {
			result.SetEffects[i].Stat = toStatValue(setEffects.at(i));
		}

		for (size_t i = 0; i < 6; i++) {
			result.UniqueEffects[i].Stat = toStatValue(uniqueEffects.at(i));
		}

		return result;
	}
}

TestData<MitamaSet, FullStatusTestData> MitamaCalculatorTestDataConverter::toTestData(const nlohmann::json& json)
{
	TestData<MitamaSet, FullStatusTestData> data{};
	data.Input = toMitamaSet(json.at("Input"));
	data.Expected = FullStatusConverter::toTestData(json.at("Expected"));
	return data;
}
