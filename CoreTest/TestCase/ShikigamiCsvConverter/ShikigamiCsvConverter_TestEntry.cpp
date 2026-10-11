#include "pch.h"

#include <gtest/gtest.h>

#include "ShikigamiCsvConverter_Test.h"

#define SHIKIGAMI_CSV_CONVERTER_TEST_LIST \
	X(T001_toShikigami_parsesValidCsv, toShikigami_parsesValidCsv) \
	X(T002_toShikigami_parsesJapaneseName, toShikigami_parsesJapaneseName) \
	X(T003_toShikigami_parsesFractionalAndNegativeValues, toShikigami_parsesFractionalAndNegativeValues) \
	X(T004_toShikigami_rejectsMissingColumns, toShikigami_rejectsMissingColumns) \
	X(T005_toShikigami_rejectsExtraColumns, toShikigami_rejectsExtraColumns) \
	X(T006_toShikigami_rejectsInvalidNumericValue, toShikigami_rejectsInvalidNumericValue) \
	X(T007_toShikigami_rejectsOutOfRangeNumericValue, toShikigami_rejectsOutOfRangeNumericValue) \
	X(T008_toShikigami_acceptsEmptyName, toShikigami_acceptsEmptyName) \
	X(T009_toCsvLine_serializesAllColumns, toCsvLine_serializesAllColumns) \
	X(T010_toCsvLine_preservesColumnOrder, toCsvLine_preservesColumnOrder) \
	X(T011_roundTrip_preservesShikigamiData, roundTrip_preservesShikigamiData)

#define X(testName, functionName) \
	TEST(ShikigamiCsvConverter_Test, testName) \
	{ \
		ShikigamiCsvConverter_Test::functionName(); \
	}

SHIKIGAMI_CSV_CONVERTER_TEST_LIST

#undef X
#undef SHIKIGAMI_CSV_CONVERTER_TEST_LIST
