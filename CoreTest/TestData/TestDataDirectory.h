#pragma once

#include <filesystem>
#include <string>

class TestDataDirectory
{
public:
	static void initialize(const std::filesystem::path& projectDirectory);
	static std::filesystem::path getPath(const std::string& fileName);
private:
	static std::string getTestTarget();
	static std::filesystem::path rootDirectory;
};
