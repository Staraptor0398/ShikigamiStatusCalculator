#include "pch.h"

#include <gtest/gtest.h>

#include "StatusComparator_Test.h"

#define STATUS_COMPARATOR_TEST_LIST \
	X(T001_compareStatus_returnsPositiveDifferences, compareStatus_returnsPositiveDifferences) \
	X(T002_compareStatus_returnsNegativeDefferences, compareStatus_returnsNegativeDefferences) \
	X(T003_compareStatus_returnsZeroDefferences, compareStatus_returnsZeroDefferences) \
	X(T004_compareStatus_returnsMixedDifferences, compareStatus_returnsMixedDifferences)

#define X(testName, functionName) \
	TEST(StatusComparator_Test, testName) \
	{ \
		StatusComparator_Test::functionName(); \
	}

STATUS_COMPARATOR_TEST_LIST

#undef X
