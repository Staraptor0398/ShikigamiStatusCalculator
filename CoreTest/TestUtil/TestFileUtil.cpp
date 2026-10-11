#include "pch.h"
#include "TestFileUtil.h"

#include <fstream>
#include <iterator>
#include <stdexcept>

void TestFileUtil::writeTextFile(const std::filesystem::path& filePath, const std::string& content)
{
	std::ofstream file(filePath, std::ios::binary | std::ios::trunc);

	if (!file.is_open()) {
		throw std::runtime_error("Failed to open test file for writing.");
	}

	file.write(content.data(), static_cast<std::streamsize>(content.size()));
	file.close();

	if (!file) {
		throw std::runtime_error("Failed to write test file.");
	}
}

std::string TestFileUtil::readTextFile(const std::filesystem::path& filePath)
{
	std::ifstream file(filePath, std::ios::binary);

	if (!file.is_open()) {
		throw std::runtime_error("Failed to open test file for reading.");
	}

	std::string content(
		std::istreambuf_iterator<char>{ file },
		std::istreambuf_iterator<char>{}
	);

	if (file.bad()) {
		throw std::runtime_error("Failed to read test file.");
	}

	return content;
}
