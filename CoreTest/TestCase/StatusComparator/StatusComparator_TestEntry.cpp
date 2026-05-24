#include "pch.h"

#include <gtest/gtest.h>

#include "StatusComparator_Test.h"

#define STATUS_COMPARATOR_TEST_LIST \
	X(T001_compareStatus_returnsAllDifferences, compareStatus_returnsAllDefferences) \
	X(T002_compareStatus_sameStatusReturnsZero, compareStatus_sameStatusReturnsZero)

#define X(testName, functionName) \
	TEST(StatusComparator_Test, testName) \
	{ \
		StatusComparator_Test::functionName(); \
	}

STATUS_COMPARATOR_TEST_LIST

#undef X
