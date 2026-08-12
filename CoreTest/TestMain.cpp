#include "pch.h"

#include <filesystem>
#include <gtest/gtest.h>

#include "TestCommon/TestResultLogger.h"
#include "TestData/TestDataDirectory.h"

std::filesystem::path findProjectDirectory();

int main(int argc, char** argv) {
	testing::InitGoogleTest(&argc, argv);

	std::filesystem::path projectDirectory;

	if (argc >= 2) {
		projectDirectory = argv[1];
	}
	else {
		projectDirectory = findProjectDirectory();
	}

	TestDataDirectory::initialize(projectDirectory);

	testing::TestEventListeners& listeners = testing::UnitTest::GetInstance()->listeners();

	listeners.Append(new TestResultLogger("TestLog/CoreTestResult.txt"));

	return RUN_ALL_TESTS();
}

std::filesystem::path findProjectDirectory()
{
	auto current = std::filesystem::current_path();

	while (true)
	{
		const auto candidate = current / "CoreTest" / "CoreTest.vcxproj";

		if (std::filesystem::exists(candidate))
		{
			return current / "CoreTest";
		}

		const auto parent = current.parent_path();

		if (parent == current)
		{
			break;
		}
		current = parent;
	}

	throw std::runtime_error("CoreTest project directory was not found.");
}
