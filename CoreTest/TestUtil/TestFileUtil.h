#pragma once

#include <filesystem>
#include <string>

class TestFileUtil
{
public:
	static void writeTextFile(const std::filesystem::path& filePath, const std::string& content);
	static std::string readTextFile(const std::filesystem::path& filePath);
};
