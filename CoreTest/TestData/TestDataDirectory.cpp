#include "pch.h"
#include "TestDataDirectory.h"

#include <gtest/gtest.h>
#include <stdexcept>

std::filesystem::path TestDataDirectory::rootDirectory{};

void TestDataDirectory::initialize(const std::filesystem::path& projectDirectory)
{
	rootDirectory = projectDirectory / "TestCase";
}

std::filesystem::path TestDataDirectory::getPath(const std::string& fileName)
{
	return rootDirectory / getTestTarget() / "_data" / fileName;
}

std::string TestDataDirectory::getTestTarget()
{
	const testing::TestInfo* testInfo = testing::UnitTest::GetInstance()->current_test_info();

	if (testInfo == nullptr)
	{
		throw std::runtime_error("Current GoogleTest information is not available.");
	}
	std::string testCaseName = testInfo->test_case_name();

	const std::string suffix = "_Test";

	if (testCaseName.size() <= suffix.size() || testCaseName.compare(testCaseName.size() - suffix.size(), suffix.size(), suffix) != 0)
	{
		throw std::runtime_error("Test suite name must end with '_Test': " + testCaseName);
	}

	testCaseName.erase(testCaseName.size() - suffix.size());

	return testCaseName;
}
