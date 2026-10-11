#include "pch.h"

#include <gtest/gtest.h>

#include "MitamaCalculator_Test.h"

#define MITAMA_CALCULATOR_TEST_LIST \
	X(T001_calculate_matchesJsonTestData, calculate_matchesJsonTestData)

#define X(testName, functionName) \
	TEST(MitamaCalculator_Test, testName) \
	{ \
		MitamaCalculator_Test::functionName(); \
	}

MITAMA_CALCULATOR_TEST_LIST

#undef X
#undef MITAMA_CALCULATOR_TEST_LIST
