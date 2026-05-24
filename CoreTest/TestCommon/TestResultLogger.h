#pragma once

#include <fstream>
#include <string>

#include<gtest/gtest.h>

class TestResultLogger : public testing::EmptyTestEventListener
{
public:
	explicit TestResultLogger(const std::string& logFilePath);

	void OnTestProgramStart(const testing::UnitTest& unitTest) override;

	void OnTestStart(const testing::TestInfo& testInfo) override;

	void OnTestPartResult(const testing::TestPartResult& testPartResult) override;

	void OnTestEnd(const testing::TestInfo& testInfo) override;

	void OnTestProgramEnd(const testing::UnitTest& unitTest) override;

private:
	std::ofstream _logFile;

	void writeLine(const std::string& message);
};
