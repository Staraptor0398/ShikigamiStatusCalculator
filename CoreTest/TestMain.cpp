#include "pch.h"

#include <gtest/gtest.h>

#include "TestCommon/TestResultLogger.h"

int main(int argc, char** argv) {
	testing::InitGoogleTest(&argc, argv);

	testing::TestEventListeners& listeners = testing::UnitTest::GetInstance()->listeners();

	listeners.Append(new TestResultLogger("TestLog/CoreTestResult.txt"));

	return RUN_ALL_TESTS();
}
