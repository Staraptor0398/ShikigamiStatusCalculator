#include "pch.h"

#include <gtest/gtest.h>

#include "MitamaCalculator_Test.h"

#define MITAMA_CALCULATOR_TEST_LIST \
	X(T001_calculateSet_addsMitamaStats, calculateSet_addsMitamaStats)

#define X(testName, functionName) \
	TEST(MitamaCalculator_Test, testName) \
	{ \
		MitamaCalculator_Test::functionName(); \
	}

MITAMA_CALCULATOR_TEST_LIST

#undef X
