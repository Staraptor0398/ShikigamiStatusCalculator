#include "pch.h"

#include "TestResultLogger.h"

#include <filesystem>

TestResultLogger::TestResultLogger(const std::string& logFilePath)
{
	std::filesystem::path path(logFilePath);
	if (path.has_parent_path())
	{
		std::filesystem::create_directories(path.parent_path());
	}
	_logFile.open(
		logFilePath,
		std::ios::out | std::ios::trunc);
}

void TestResultLogger::OnTestProgramStart(const testing::UnitTest& unitTest)
{
	writeLine("[==========] CoreTest Start");
	writeLine("Test count: " + std::to_string(unitTest.test_to_run_count()));
}
void TestResultLogger::OnTestStart(const testing::TestInfo& testInfo)
{
	writeLine(
		"[ RUN      ] " +
		std::string(testInfo.test_case_name()) +
		"." +
		testInfo.name());
}

void TestResultLogger::OnTestPartResult(const testing::TestPartResult& testPartResult)
{
	if (testPartResult.failed())
	{
		writeLine(
			"[  FAILED  ] " +
			std::string(testPartResult.file_name()) +
			":" +
			std::to_string(testPartResult.line_number()));
		writeLine(testPartResult.summary());
	}
}

void TestResultLogger::OnTestEnd(const testing::TestInfo& testInfo)
{
	std::string resultText =
		testInfo.result()->Passed()
		? "[       OK ] "
		: "[  FAILED  ] ";
	writeLine(
		resultText +
		std::string(testInfo.test_case_name()) +
		"." +
		testInfo.name());
}

void TestResultLogger::OnTestProgramEnd(const testing::UnitTest& unitTest)
{
	writeLine("[==========] CoreTest End");
	writeLine("Passed: " + std::to_string(unitTest.successful_test_count()));
	writeLine("Failed: " + std::to_string(unitTest.failed_test_count()));
}

void TestResultLogger::writeLine(const std::string& message)
{
	if (_logFile.is_open())
	{
		_logFile << message << std::endl;
	}
}
