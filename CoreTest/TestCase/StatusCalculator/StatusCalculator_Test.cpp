#include "pch.h"
#include "StatusCalculator_Test.h"

#include "../../../Core/Model/Status.h"
#include "../../../Core/Status/StatusCalculator.h"
#include "../../TestCommon/TestAssert.h"

void StatusCalculator_Test::calculateFinalStatus_addsBaseAndMitamaStatus()
{
	Status baseStatus{};
	baseStatus.Attack = 100.0;
	baseStatus.Hp = 1000.0;
	baseStatus.Defense = 200.0;
	baseStatus.Speed = 110.0;
	baseStatus.CriticalRate = 10.0;
	baseStatus.CriticalDamage = 150.0;
	baseStatus.EffectHit = 20.0;
	baseStatus.EffectResist = 30.0;

	Status mitamaOnlyStatus{};
	mitamaOnlyStatus.Attack = 50.0;
	mitamaOnlyStatus.Hp = 300.0;
	mitamaOnlyStatus.Defense = 40.0;
	mitamaOnlyStatus.Speed = 18.0;
	mitamaOnlyStatus.CriticalRate = 15.0;
	mitamaOnlyStatus.CriticalDamage = 25.0;
	mitamaOnlyStatus.EffectHit = 12.0;
	mitamaOnlyStatus.EffectResist = 8.0;

	Status result = StatusCalculator::calculateFinalStatus(baseStatus, mitamaOnlyStatus);

	TEST_ASSERT_DOUBLE_EQUAL(150.0, result.Attack);
	TEST_ASSERT_DOUBLE_EQUAL(1300.0, result.Hp);
	TEST_ASSERT_DOUBLE_EQUAL(240.0, result.Defense);
	TEST_ASSERT_DOUBLE_EQUAL(128.0, result.Speed);
	TEST_ASSERT_DOUBLE_EQUAL(25.0, result.CriticalRate);
	TEST_ASSERT_DOUBLE_EQUAL(175.0, result.CriticalDamage);
	TEST_ASSERT_DOUBLE_EQUAL(32.0, result.EffectHit);
	TEST_ASSERT_DOUBLE_EQUAL(38.0, result.EffectResist);
}
