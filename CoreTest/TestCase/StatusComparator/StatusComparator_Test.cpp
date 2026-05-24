#include "pch.h"
#include "StatusComparator_Test.h"

#include "../../../Core/Model/Status.h"
#include "../../../Core/Status/StatusComparator.h"
#include "../../TestCommon/TestAssert.h"

void StatusComparator_Test::compareStatus_returnsPositiveDifferences()
{
	Status baseStatus{};
	baseStatus.Attack = 100.0;
	baseStatus.Hp = 1000.0;
	baseStatus.Defense = 200.0;
	baseStatus.Speed = 100.0;
	baseStatus.CriticalRate = 10.0;
	baseStatus.CriticalDamage = 150.0;
	baseStatus.EffectHit = 20.0;
	baseStatus.EffectResist = 30.0;

	Status targetStatus{};
	targetStatus.Attack = 150.0;
	targetStatus.Hp = 1200.0;
	targetStatus.Defense = 250.0;
	targetStatus.Speed = 120.0;
	targetStatus.CriticalRate = 15.0;
	targetStatus.CriticalDamage = 180.0;
	targetStatus.EffectHit = 25.0;
	targetStatus.EffectResist = 40.0;

	StatusComparisonResult result = StatusComparator::Compare(baseStatus, targetStatus);

	TEST_ASSERT_DOUBLE_EQUAL(50.0, result.AttackDifferense);
	TEST_ASSERT_DOUBLE_EQUAL(200.0, result.HpDifferense);
	TEST_ASSERT_DOUBLE_EQUAL(50.0, result.DefenseDifferense);
	TEST_ASSERT_DOUBLE_EQUAL(20.0, result.SpeedDifferense);
	TEST_ASSERT_DOUBLE_EQUAL(5.0, result.CriticalRateDifferense);
	TEST_ASSERT_DOUBLE_EQUAL(30.0, result.CriticalDamageDifferense);
	TEST_ASSERT_DOUBLE_EQUAL(5.0, result.EffectHitDifferense);
	TEST_ASSERT_DOUBLE_EQUAL(10.0, result.EffectResistDifferense);
}

void StatusComparator_Test::compareStatus_returnsNegativeDefferences()
{
	Status baseStatus{};
	baseStatus.Attack = 150.0;
	baseStatus.Hp = 1200.0;
	baseStatus.Defense = 250.0;
	baseStatus.Speed = 120.0;
	baseStatus.CriticalRate = 15.0;
	baseStatus.CriticalDamage = 180.0;
	baseStatus.EffectHit = 25.0;
	baseStatus.EffectResist = 40.0;

	Status targetStatus{};
	targetStatus.Attack = 100.0;
	targetStatus.Hp = 1000.0;
	targetStatus.Defense = 200.0;
	targetStatus.Speed = 100.0;
	targetStatus.CriticalRate = 10.0;
	targetStatus.CriticalDamage = 150.0;
	targetStatus.EffectHit = 20.0;
	targetStatus.EffectResist = 30.0;

	StatusComparisonResult result = StatusComparator::Compare(baseStatus, targetStatus);

	TEST_ASSERT_DOUBLE_EQUAL(-50.0, result.AttackDifferense);
	TEST_ASSERT_DOUBLE_EQUAL(-200.0, result.HpDifferense);
	TEST_ASSERT_DOUBLE_EQUAL(-50.0, result.DefenseDifferense);
	TEST_ASSERT_DOUBLE_EQUAL(-20.0, result.SpeedDifferense);
	TEST_ASSERT_DOUBLE_EQUAL(-5.0, result.CriticalRateDifferense);
	TEST_ASSERT_DOUBLE_EQUAL(-30.0, result.CriticalDamageDifferense);
	TEST_ASSERT_DOUBLE_EQUAL(-5.0, result.EffectHitDifferense);
	TEST_ASSERT_DOUBLE_EQUAL(-10.0, result.EffectResistDifferense);
}

void StatusComparator_Test::compareStatus_returnsZeroDefferences()
{
	Status baseStatus{};
	baseStatus.Attack = 100.0;
	baseStatus.Hp = 1000.0;
	baseStatus.Defense = 200.0;
	baseStatus.Speed = 100.0;
	baseStatus.CriticalRate = 10.0;
	baseStatus.CriticalDamage = 150.0;
	baseStatus.EffectHit = 20.0;
	baseStatus.EffectResist = 30.0;

	Status targetStatus = baseStatus;
	StatusComparisonResult result = StatusComparator::Compare(baseStatus, targetStatus);

	TEST_ASSERT_DOUBLE_EQUAL(0.0, result.AttackDifferense);
	TEST_ASSERT_DOUBLE_EQUAL(0.0, result.HpDifferense);
	TEST_ASSERT_DOUBLE_EQUAL(0.0, result.DefenseDifferense);
	TEST_ASSERT_DOUBLE_EQUAL(0.0, result.SpeedDifferense);
	TEST_ASSERT_DOUBLE_EQUAL(0.0, result.CriticalRateDifferense);
	TEST_ASSERT_DOUBLE_EQUAL(0.0, result.CriticalDamageDifferense);
	TEST_ASSERT_DOUBLE_EQUAL(0.0, result.EffectHitDifferense);
	TEST_ASSERT_DOUBLE_EQUAL(0.0, result.EffectResistDifferense);
}

void StatusComparator_Test::compareStatus_returnsMixedDifferences()
{
	Status baseStatus{};
	baseStatus.Attack = 100.0;
	baseStatus.Hp = 1000.0;
	baseStatus.Defense = 200.0;
	baseStatus.Speed = 100.0;
	baseStatus.CriticalRate = 10.0;
	baseStatus.CriticalDamage = 150.0;
	baseStatus.EffectHit = 20.0;
	baseStatus.EffectResist = 30.0;

	Status targetStatus{};
	targetStatus.Attack = 150.0;
	targetStatus.Hp = 800.0;
	targetStatus.Defense = 250.0;
	targetStatus.Speed = 100.0;
	targetStatus.CriticalRate = 15.0;
	targetStatus.CriticalDamage = 180.0;
	targetStatus.EffectHit = 20.0;
	targetStatus.EffectResist = 10.0;

	StatusComparisonResult result = StatusComparator::Compare(baseStatus, targetStatus);

	TEST_ASSERT_DOUBLE_EQUAL(50.0, result.AttackDifferense);
	TEST_ASSERT_DOUBLE_EQUAL(-200.0, result.HpDifferense);
	TEST_ASSERT_DOUBLE_EQUAL(50.0, result.DefenseDifferense);
	TEST_ASSERT_DOUBLE_EQUAL(0.0, result.SpeedDifferense);
	TEST_ASSERT_DOUBLE_EQUAL(5.0, result.CriticalRateDifferense);
	TEST_ASSERT_DOUBLE_EQUAL(30.0, result.CriticalDamageDifferense);
	TEST_ASSERT_DOUBLE_EQUAL(0.0, result.EffectHitDifferense);
	TEST_ASSERT_DOUBLE_EQUAL(-20.0, result.EffectResistDifferense);
}
