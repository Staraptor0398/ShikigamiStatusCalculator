#include "pch.h"
#include "ShikigamiCsvConverter_Test.h"

#include "../../../Core/Converter/Format/ShikigamiCsvConverter.h"
#include "../../TestCommon/TestAssert.h"

#include <gtest/gtest.h>
#include <stdexcept>
#include <string>

namespace {
	const std::string VALID_CSV_LINE = "SSR,Test,1234.5,5678.25,321.75,112.5,10.25,150.5,20.75,30.125";

	Shikigami createShikigami()
	{
		Shikigami shikigami;
		shikigami.Rarity = "SP";
		shikigami.Name = "Test";
		shikigami.Status.Attack = 1234.5;
		shikigami.Status.Hp = 5678.25;
		shikigami.Status.Defense = 321.75;
		shikigami.Status.Speed = 112.5;
		shikigami.Status.CriticalRate = 10.25;
		shikigami.Status.CriticalDamage = 150.5;
		shikigami.Status.EffectHit = 20.75;
		shikigami.Status.EffectResist = 30.125;
		return shikigami;
	}

	void assertShikigamiEqual(const Shikigami& expected, const Shikigami& actual)
	{
		TEST_ASSERT_EQUAL(expected.Rarity.toString(), actual.Rarity.toString());
		TEST_ASSERT_EQUAL(expected.Name, actual.Name);
		TEST_ASSERT_DOUBLE_EQUAL(expected.Status.Attack, actual.Status.Attack);
		TEST_ASSERT_DOUBLE_EQUAL(expected.Status.Hp, actual.Status.Hp);
		TEST_ASSERT_DOUBLE_EQUAL(expected.Status.Defense, actual.Status.Defense);
		TEST_ASSERT_DOUBLE_EQUAL(expected.Status.Speed, actual.Status.Speed);
		TEST_ASSERT_DOUBLE_EQUAL(expected.Status.CriticalRate, actual.Status.CriticalRate);
		TEST_ASSERT_DOUBLE_EQUAL(expected.Status.CriticalDamage, actual.Status.CriticalDamage);
		TEST_ASSERT_DOUBLE_EQUAL(expected.Status.EffectHit, actual.Status.EffectHit);
		TEST_ASSERT_DOUBLE_EQUAL(expected.Status.EffectResist, actual.Status.EffectResist);
	}
}

void ShikigamiCsvConverter_Test::toShikigami_parsesValidCsv()
{
	Shikigami actual = ShikigamiCsvConverter::toShikigami(VALID_CSV_LINE);
	Shikigami expected = createShikigami();
	expected.Rarity = "SSR";
	assertShikigamiEqual(expected, actual);
}

void ShikigamiCsvConverter_Test::toShikigami_parsesJapaneseName()
{
	Shikigami actual = ShikigamiCsvConverter::toShikigami("SP,不見岳,100,2000,200,110,10,150,20,30");
	TEST_ASSERT_EQUAL("SP", actual.Rarity.toString());
	TEST_ASSERT_EQUAL("不見岳", actual.Name);
	TEST_ASSERT_DOUBLE_EQUAL(100.0, actual.Status.Attack);
}

void ShikigamiCsvConverter_Test::toShikigami_parsesFractionalAndNegativeValues()
{
	Shikigami actual = ShikigamiCsvConverter::toShikigami("SR,Test,-1.5,0.25,3.75,100,10,150,20,30");
	TEST_ASSERT_DOUBLE_EQUAL(-1.5, actual.Status.Attack);
	TEST_ASSERT_DOUBLE_EQUAL(0.25, actual.Status.Hp);
	TEST_ASSERT_DOUBLE_EQUAL(3.75, actual.Status.Defense);
}

void ShikigamiCsvConverter_Test::toShikigami_rejectsMissingColumns()
{
	EXPECT_THROW(ShikigamiCsvConverter::toShikigami("SSR,Test,100,2000,200,110,10,150,20"), std::invalid_argument);
}

void ShikigamiCsvConverter_Test::toShikigami_rejectsExtraColumns()
{
	EXPECT_THROW(ShikigamiCsvConverter::toShikigami("SSR,Test,100,2000,200,110,10,150,20,30,Extra"), std::invalid_argument);
}

void ShikigamiCsvConverter_Test::toShikigami_rejectsInvalidNumericValue()
{
	EXPECT_THROW(ShikigamiCsvConverter::toShikigami("SSR,Test,NotANumber,2000,200,110,10,150,20,30"), std::invalid_argument);
}

void ShikigamiCsvConverter_Test::toShikigami_rejectsOutOfRangeNumericValue()
{
	EXPECT_THROW(ShikigamiCsvConverter::toShikigami("SSR,Test,1e9999,2000,200,110,10,150,20,30"), std::out_of_range);
}

void ShikigamiCsvConverter_Test::toShikigami_acceptsEmptyName()
{
	Shikigami actual = ShikigamiCsvConverter::toShikigami("SSR,,100,2000,200,110,10,150,20,30");
	TEST_ASSERT_EQUAL("", actual.Name);
	TEST_ASSERT_EQUAL("SSR", actual.Rarity.toString());
}

void ShikigamiCsvConverter_Test::toCsvLine_serializesAllColumns()
{
	Shikigami value = createShikigami();
	std::string actual = ShikigamiCsvConverter::toCsvLine(value);
	const std::string expected = "SP,Test,1234.500000,5678.250000,321.750000,112.500000,10.250000,150.500000,20.750000,30.125000";
	TEST_ASSERT_EQUAL(expected, actual);
}

void ShikigamiCsvConverter_Test::toCsvLine_preservesColumnOrder()
{
	Shikigami value;
	value.Rarity = "UR";
	value.Name = "ColumnOrder";
	value.Status.Attack = 1;
	value.Status.Hp = 2;
	value.Status.Defense = 3;
	value.Status.Speed = 4;
	value.Status.CriticalRate = 5;
	value.Status.CriticalDamage = 6;
	value.Status.EffectHit = 7;
	value.Status.EffectResist = 8;
	std::string actual = ShikigamiCsvConverter::toCsvLine(value);
	TEST_ASSERT_EQUAL("UR,ColumnOrder,1.000000,2.000000,3.000000,4.000000,5.000000,6.000000,7.000000,8.000000", actual);
}

void ShikigamiCsvConverter_Test::roundTrip_preservesShikigamiData()
{
	Shikigami expected = createShikigami();
	expected.Name = "式神テスト";
	Shikigami actual = ShikigamiCsvConverter::toShikigami(ShikigamiCsvConverter::toCsvLine(expected));
	assertShikigamiEqual(expected, actual);
}
