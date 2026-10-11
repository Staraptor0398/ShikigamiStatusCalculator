#include "pch.h"
#include "FileAccess_Test.h"

#include "../../../Core/IO/FileAccess.h"
#include "../../TestCommon/TestAssert.h"
#include "../../TestUtil/TemporaryDirectory.h"
#include "../../TestUtil/TestFileUtil.h"

#include <Windows.h>
#include <filesystem>
#include <string>
#include <vector>

namespace {
	const std::string CSV_HEADER = "レア度,式神名,攻撃力,HP,防御力,素早さ,会心率,会心DMG,効果命中,効果抵抗";

	Shikigami createShikigami(const std::string& rarity, const std::string& name, double attack)
	{
		Shikigami shikigami;
		shikigami.Rarity = rarity;
		shikigami.Name = name;
		shikigami.Status.Attack = attack;
		shikigami.Status.Hp = 1000.0;
		shikigami.Status.Defense = 200.0;
		shikigami.Status.Speed = 100.0;
		shikigami.Status.CriticalRate = 10.0;
		shikigami.Status.CriticalDamage = 150.0;
		shikigami.Status.EffectHit = 20.0;
		shikigami.Status.EffectResist = 30.0;
		return shikigami;
	}

	std::string createCsvLine(const std::string& rarity, const std::string& name, double attack)
	{
		return rarity + "," + name + "," + std::to_string(attack) + ",1000,200,100,10,150,20,30";
	}

	void assertShikigamiEqual(const Shikigami& expected, const Shikigami& actual)
	{
		TEST_ASSERT_EQUAL(expected.Rarity.toString(), actual.Rarity.toString());
		TEST_ASSERT_EQUAL(expected.Name, actual.Name);
		TEST_ASSERT_DOUBLE_EQUAL(expected.Status.Attack, actual.Status.Attack);
		TEST_ASSERT_DOUBLE_EQUAL(expected.Status.Hp, actual.Status.Hp);
		TEST_ASSERT_DOUBLE_EQUAL(expected.Status.Defense, actual.Status.Defense);
		TEST_ASSERT_DOUBLE_EQUAL(expected.Status.Speed, actual.Status.Speed);
		TEST_ASSERT_DOUBLE_EQUAL(expected.Status.CriticalRate, actual.Status.CriticalRate);
		TEST_ASSERT_DOUBLE_EQUAL(expected.Status.CriticalDamage, actual.Status.CriticalDamage);
		TEST_ASSERT_DOUBLE_EQUAL(expected.Status.EffectHit, actual.Status.EffectHit);
		TEST_ASSERT_DOUBLE_EQUAL(expected.Status.EffectResist, actual.Status.EffectResist);
	}

	class FileHandleGuard
	{
	public:
		explicit FileHandleGuard(HANDLE handle) : mHandle(handle) {}
		~FileHandleGuard()
		{
			if (mHandle != INVALID_HANDLE_VALUE && mHandle != nullptr) {
				CloseHandle(mHandle);
			}
		}
		FileHandleGuard(const FileHandleGuard&) = delete;
		FileHandleGuard& operator=(const FileHandleGuard&) = delete;
	private:
		HANDLE mHandle;
	};
}

void FileAccess_Test::saveShikigami_replacesExistingCsv()
{
	TemporaryDirectory directory;
	std::filesystem::path filePath = directory.getPath() / "ShikigamiData.csv";
	const std::string originalContent = CSV_HEADER + "\n" + createCsvLine("SSR", "Original", 100.0) + "\n";
	TestFileUtil::writeTextFile(filePath, originalContent);
	std::vector<Shikigami> shikigamis = { createShikigami("SP", "NewShikigami", 200.0) };
	FileAccessOutcome outcome = FileAccess::saveShikigami(filePath.string(), shikigamis);
	ASSERT_EQ(FileAccessOutcome::SUCCESS, outcome);
	std::vector<Shikigami> loadedShikigamis;
	FileAccessOutcome loadOutcome = FileAccess::loadShikigami(filePath.string(), loadedShikigamis);
	ASSERT_EQ(FileAccessOutcome::SUCCESS, loadOutcome);
	ASSERT_EQ(1u, loadedShikigamis.size());
	assertShikigamiEqual(shikigamis[0], loadedShikigamis[0]);
	TEST_ASSERT_NOT_EQUAL(originalContent, TestFileUtil::readTextFile(filePath));
}

void FileAccess_Test::saveShikigami_preservesDataAfterReload()
{
	TemporaryDirectory directory;
	std::filesystem::path filePath = directory.getPath() / "ShikigamiData.csv";
	TestFileUtil::writeTextFile(filePath, CSV_HEADER + "\n");
	std::vector<Shikigami> expectedShikigamis = {
		createShikigami("SP", "First", 100.0),
		createShikigami("SSR", "Second", 200.0),
		createShikigami("SR", "Third", 300.0)
	};
	FileAccessOutcome saveOutcome = FileAccess::saveShikigami(filePath.string(), expectedShikigamis);
	ASSERT_EQ(FileAccessOutcome::SUCCESS, saveOutcome);
	std::vector<Shikigami> actualShikigamis;
	FileAccessOutcome loadOutcome = FileAccess::loadShikigami(filePath.string(), actualShikigamis);
	ASSERT_EQ(FileAccessOutcome::SUCCESS, loadOutcome);
	ASSERT_EQ(expectedShikigamis.size(), actualShikigamis.size());
	for (size_t i = 0; i < expectedShikigamis.size(); i++) {
		assertShikigamiEqual(expectedShikigamis[i], actualShikigamis[i]);
	}
}

void FileAccess_Test::saveShikigami_preservesOriginalWhenReplacementFails()
{
	TemporaryDirectory directory;
	std::filesystem::path filePath = directory.getPath() / "ShikigamiData.csv";
	const std::string originalContent = CSV_HEADER + "\n" + createCsvLine("SSR", "Original", 100.0) + "\n";
	TestFileUtil::writeTextFile(filePath, originalContent);
	std::vector<Shikigami> shikigamis = { createShikigami("SP", "Replacement", 999.0) };
	FileAccessOutcome outcome;
	{
		HANDLE handle = CreateFileW(filePath.c_str(), GENERIC_READ, 0, nullptr, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, nullptr);
		ASSERT_NE(INVALID_HANDLE_VALUE, handle);
		FileHandleGuard guard(handle);
		outcome = FileAccess::saveShikigami(filePath.string(), shikigamis);
	}
	TEST_ASSERT_EQUAL(FileAccessOutcome::FILE_WRITE_FAILED, outcome);
	TEST_ASSERT_EQUAL(originalContent, TestFileUtil::readTextFile(filePath));
}

void FileAccess_Test::saveShikigami_failsWhenTemporaryDirectoryDoesNotExist()
{
	TemporaryDirectory directory;
	std::filesystem::path filePath = directory.getPath() / "MissingDirectory" / "ShikigamiData.csv";
	std::vector<Shikigami> shikigamis = { createShikigami("SSR", "Test", 100.0) };
	FileAccessOutcome outcome = FileAccess::saveShikigami(filePath.string(), shikigamis);
	TEST_ASSERT_EQUAL(FileAccessOutcome::FILE_OPEN_FAILED, outcome);
	TEST_ASSERT_FALSE(std::filesystem::exists(filePath));
}

void FileAccess_Test::saveShikigami_failsWhenDestinationDoesNotExist()
{
	TemporaryDirectory directory;
	std::filesystem::path filePath = directory.getPath() / "ShikigamiData.csv";
	std::vector<Shikigami> shikigamis = { createShikigami("SSR", "Test", 100.0) };
	FileAccessOutcome outcome = FileAccess::saveShikigami(filePath.string(), shikigamis);
	TEST_ASSERT_EQUAL(FileAccessOutcome::FILE_WRITE_FAILED, outcome);
	TEST_ASSERT_FALSE(std::filesystem::exists(filePath));
}

void FileAccess_Test::loadShikigami_readsValidCsv()
{
	TemporaryDirectory directory;
	std::filesystem::path filePath = directory.getPath() / "ShikigamiData.csv";
	std::string csv = CSV_HEADER + "\n" + createCsvLine("SSR", "First", 100.0) + "\n" + createCsvLine("SR", "Second", 200.0) + "\n";
	TestFileUtil::writeTextFile(filePath, csv);
	std::vector<Shikigami> shikigamis;
	FileAccessOutcome outcome = FileAccess::loadShikigami(filePath.string(), shikigamis);
	ASSERT_EQ(FileAccessOutcome::SUCCESS, outcome);
	ASSERT_EQ(2u, shikigamis.size());
	TEST_ASSERT_EQUAL("SSR", shikigamis[0].Rarity.toString());
	TEST_ASSERT_EQUAL("First", shikigamis[0].Name);
	TEST_ASSERT_DOUBLE_EQUAL(100.0, shikigamis[0].Status.Attack);
	TEST_ASSERT_EQUAL("SR", shikigamis[1].Rarity.toString());
	TEST_ASSERT_EQUAL("Second", shikigamis[1].Name);
	TEST_ASSERT_DOUBLE_EQUAL(200.0, shikigamis[1].Status.Attack);
}

void FileAccess_Test::loadShikigami_rejectsInvalidHeader()
{
	TemporaryDirectory directory;
	std::filesystem::path filePath = directory.getPath() / "ShikigamiData.csv";
	TestFileUtil::writeTextFile(filePath, "InvalidHeader\n" + createCsvLine("SSR", "Test", 100.0) + "\n");
	std::vector<Shikigami> shikigamis;
	FileAccessOutcome outcome = FileAccess::loadShikigami(filePath.string(), shikigamis);
	TEST_ASSERT_EQUAL(FileAccessOutcome::INVALID_FORMAT, outcome);
	TEST_ASSERT_TRUE(shikigamis.empty());
}

void FileAccess_Test::loadShikigami_rejectsInvalidRecord()
{
	TemporaryDirectory directory;
	std::filesystem::path filePath = directory.getPath() / "ShikigamiData.csv";
	TestFileUtil::writeTextFile(filePath, CSV_HEADER + "\n" + createCsvLine("SSR", "Valid", 100.0) + "\n" + "SSR,Invalid,NotANumber,1000,200,100,10,150,20,30\n");
	std::vector<Shikigami> shikigamis;
	FileAccessOutcome outcome = FileAccess::loadShikigami(filePath.string(), shikigamis);
	TEST_ASSERT_EQUAL(FileAccessOutcome::INVALID_FORMAT, outcome);
	TEST_ASSERT_TRUE(shikigamis.empty());
}

void FileAccess_Test::loadShikigami_failsWhenFileDoesNotExist()
{
	TemporaryDirectory directory;
	std::filesystem::path filePath = directory.getPath() / "Missing.csv";
	std::vector<Shikigami> shikigamis;
	FileAccessOutcome outcome = FileAccess::loadShikigami(filePath.string(), shikigamis);
	TEST_ASSERT_EQUAL(FileAccessOutcome::FILE_OPEN_FAILED, outcome);
	TEST_ASSERT_TRUE(shikigamis.empty());
}

void FileAccess_Test::loadValidShikigami_skipsInvalidRecords()
{
	TemporaryDirectory directory;
	std::filesystem::path filePath = directory.getPath() / "ShikigamiData.csv";
	std::string csv = CSV_HEADER + "\n" + createCsvLine("SSR", "First", 100.0) + "\n" + "InvalidRecord\n" + createCsvLine("SR", "Second", 200.0) + "\n" + "SSR,InvalidNumber,ABC,1000,200,100,10,150,20,30\n";
	TestFileUtil::writeTextFile(filePath, csv);
	std::vector<Shikigami> shikigamis;
	FileAccessOutcome outcome = FileAccess::loadValidShikigami(filePath.string(), shikigamis);
	ASSERT_EQ(FileAccessOutcome::SUCCESS, outcome);
	ASSERT_EQ(2u, shikigamis.size());
	TEST_ASSERT_EQUAL("First", shikigamis[0].Name);
	TEST_ASSERT_EQUAL("Second", shikigamis[1].Name);
}
