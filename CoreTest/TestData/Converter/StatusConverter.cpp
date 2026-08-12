#include "pch.h"
#include "StatusConverter.h"

StatusTestData StatusConverter::toTestData(const nlohmann::json& json)
{
	StatusTestData data{};

	data.Attack = json.at("Attack").get<double>();
	data.HP = json.at("HP").get<double>();
	data.Defense = json.at("Defense").get<double>();
	data.Speed = json.at("Speed").get<double>();
	data.CriticalRate = json.at("CritRate").get<double>();
	data.CriticalDamage = json.at("CritDamage").get<double>();
	data.EffectHit = json.at("EffectHit").get<double>();
	data.EffectResist = json.at("EffectResist").get<double>();

	return data;
}

Status StatusConverter::toNative(const StatusTestData& testData)
{
	Status status{};

	status.Attack = testData.Attack;
	status.Hp = testData.HP;
	status.Defense = testData.Defense;
	status.Speed = testData.Speed;
	status.CriticalRate = testData.CriticalRate;
	status.CriticalDamage = testData.CriticalDamage;
	status.EffectHit = testData.EffectHit;
	status.EffectResist = testData.EffectResist;

	return status;
}
