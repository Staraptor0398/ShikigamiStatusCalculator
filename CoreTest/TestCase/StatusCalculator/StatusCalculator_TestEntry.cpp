#include "pch.h"

#include <gtest/gtest.h>

#include "StatusCalculator_Test.h"

#define STATUS_CALCULATOR_TEST_LIST \
	X(T001_calculateFinalStatus_matchesJsonTestData, calculateFinalStatus_matchesJsonTestData)

#define X(testName, functionName) \
	TEST(StatusCalculator_Test, testName) \
	{ \
		StatusCalculator_Test::functionName(); \
	}

STATUS_CALCULATOR_TEST_LIST

#undef X
#undef STATUS_CALCULATOR_TEST_LIST
