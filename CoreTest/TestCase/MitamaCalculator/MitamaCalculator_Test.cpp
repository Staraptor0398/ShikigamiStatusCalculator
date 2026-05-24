#include "pch.h"
#include "MitamaCalculator_Test.h"

#include "../../../Core/Mitama/MitamaCalculator.h"
#include "../../TestCommon/TestAssert.h"
#include "_data/MitamaCalculatorTestData.h"

void MitamaCalculator_Test::calculateSet_addsMitamaStats()
{
	MitamaSet mitamaSet = MitamaCalculatorTestData::createValidMitamaSetTestData();

	Status result = MitamaCalculator::calclateSet(mitamaSet);

	TEST_ASSERT_DOUBLE_EQUAL(553.0, result.Attack);
	TEST_ASSERT_DOUBLE_EQUAL(2502.0, result.Hp);
	TEST_ASSERT_DOUBLE_EQUAL(150.0, result.Defense);
	TEST_ASSERT_DOUBLE_EQUAL(72.0, result.Speed);
	TEST_ASSERT_DOUBLE_EQUAL(20.0, result.CriticalRate);
	TEST_ASSERT_DOUBLE_EQUAL(107.0, result.CriticalDamage);
	TEST_ASSERT_DOUBLE_EQUAL(78.0, result.EffectHit);
	TEST_ASSERT_DOUBLE_EQUAL(15.0, result.EffectResist);
	TEST_ASSERT_DOUBLE_EQUAL(25.0, result.AdditionalAttackRate);
	TEST_ASSERT_DOUBLE_EQUAL(27.0, result.AdditionalHpRate);
	TEST_ASSERT_DOUBLE_EQUAL(15.0, result.AdditionalDeffenseRate);
}
