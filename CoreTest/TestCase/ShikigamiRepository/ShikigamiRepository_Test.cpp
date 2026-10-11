#include "pch.h"
#include "ShikigamiRepository_Test.h"

#include "../../../Core/Converter/Format/ShikigamiCsvConverter.h"
#include "../../../Core/Shikigami/ShikigamiRepository.h"
#include "../../TestCommon/TestAssert.h"
#include "../../TestUtil/TemporaryDirectory.h"
#include "../../TestUtil/TestFileUtil.h"

#include <filesystem>
#include <string>
#include <vector>

namespace {
	const std::string CSV_HEADER = "レア度,式神名,攻撃力,HP,防御力,素早さ,会心率,会心DMG,効果命中,効果抵抗";

	Shikigami createShikigami(const std::string& rarity, const std::string& name, double attack = 100.0)
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

	void writeCsv(const std::filesystem::path& filePath, const std::vector<Shikigami>& shikigamis)
	{
		std::string content = CSV_HEADER + "\n";

		for (const auto& shikigami : shikigamis) {
			content += ShikigamiCsvConverter::toCsvLine(shikigami) + "\n";
		}

		TestFileUtil::writeTextFile(filePath, content);
	}


	std::vector<Shikigami> loadCsv(const std::filesystem::path& filePath)
	{
		std::vector<Shikigami> shikigamis;
		EXPECT_EQ(ShikigamiDataOutcome::SUCCESS, ShikigamiRepository::getShikigamiList(filePath.string(), shikigamis));
		return shikigamis;
	}

	void assertShikigami(const Shikigami& expected, const Shikigami& actual)
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
}

void ShikigamiRepository_Test::getShikigamiList_readsExistingData()
{
	TemporaryDirectory directory;
	std::filesystem::path path = directory.getPath() / "ShikigamiData.csv";
	Shikigami expected = createShikigami("SSR", "Existing", 321.0);
	writeCsv(path, { expected });

	std::vector<Shikigami> actual;
	ShikigamiDataOutcome outcome = ShikigamiRepository::getShikigamiList(path.string(), actual);

	ASSERT_EQ(ShikigamiDataOutcome::SUCCESS, outcome);
	ASSERT_EQ(1u, actual.size());
	assertShikigami(expected, actual[0]);
}

void ShikigamiRepository_Test::getShikigamiList_failsWhenFileIsMissing()
{
	TemporaryDirectory directory;
	std::filesystem::path path = directory.getPath() / "Missing.csv";
	std::vector<Shikigami> actual;

	TEST_ASSERT_EQUAL(ShikigamiDataOutcome::FILE_OPEN_FAILED, ShikigamiRepository::getShikigamiList(path.string(), actual));
	TEST_ASSERT_TRUE(actual.empty());
}

void ShikigamiRepository_Test::addShikigami_insertsInRarityOrder()
{
	TemporaryDirectory directory;
	std::filesystem::path path = directory.getPath() / "ShikigamiData.csv";
	writeCsv(path, { createShikigami("SP", "Top"), createShikigami("SR", "Bottom") });

	ShikigamiDataOutcome outcome = ShikigamiRepository::addShikigami(path.string(), createShikigami("SSR", "Middle"));
	ASSERT_EQ(ShikigamiDataOutcome::SUCCESS, outcome);

	std::vector<Shikigami> actual = loadCsv(path);
	ASSERT_EQ(3u, actual.size());
	TEST_ASSERT_EQUAL("Top", actual[0].Name);
	TEST_ASSERT_EQUAL("Middle", actual[1].Name);
	TEST_ASSERT_EQUAL("Bottom", actual[2].Name);
}

void ShikigamiRepository_Test::addShikigami_rejectsDuplicateRarityAndName()
{
	TemporaryDirectory directory;
	std::filesystem::path path = directory.getPath() / "ShikigamiData.csv";
	writeCsv(path, { createShikigami("SSR", "Existing", 100.0) });
	std::string original = TestFileUtil::readTextFile(path);

	ShikigamiDataOutcome outcome = ShikigamiRepository::addShikigami(path.string(), createShikigami("SSR", "Existing", 999.0));

	TEST_ASSERT_EQUAL(ShikigamiDataOutcome::DUPLICATE, outcome);
	TEST_ASSERT_EQUAL(original, TestFileUtil::readTextFile(path));
}

void ShikigamiRepository_Test::addShikigami_allowsSameNameWithDifferentRarity()
{
	TemporaryDirectory directory;
	std::filesystem::path path = directory.getPath() / "ShikigamiData.csv";
	writeCsv(path, { createShikigami("SSR", "SharedName") });

	ASSERT_EQ(ShikigamiDataOutcome::SUCCESS, ShikigamiRepository::addShikigami(path.string(), createShikigami("SP", "SharedName")));
	std::vector<Shikigami> actual = loadCsv(path);
	ASSERT_EQ(2u, actual.size());
	TEST_ASSERT_EQUAL("SP", actual[0].Rarity.toString());
	TEST_ASSERT_EQUAL("SSR", actual[1].Rarity.toString());
}

void ShikigamiRepository_Test::addShikigami_preservesInvalidCsv()
{
	TemporaryDirectory directory;
	std::filesystem::path path = directory.getPath() / "ShikigamiData.csv";
	TestFileUtil::writeTextFile(path, "InvalidHeader\nSSR,Existing,100,1000,200,100,10,150,20,30\n");
	std::string original = TestFileUtil::readTextFile(path);

	ShikigamiDataOutcome outcome = ShikigamiRepository::addShikigami(path.string(), createShikigami("SP", "New"));

	TEST_ASSERT_EQUAL(ShikigamiDataOutcome::INVALID_FORMAT, outcome);
	TEST_ASSERT_EQUAL(original, TestFileUtil::readTextFile(path));
}

void ShikigamiRepository_Test::updateShikigami_updatesExistingData()
{
	TemporaryDirectory directory;
	std::filesystem::path path = directory.getPath() / "ShikigamiData.csv";
	Shikigami original = createShikigami("SSR", "Before", 100.0);
	Shikigami updated = createShikigami("SSR", "After", 350.0);
	writeCsv(path, { createShikigami("SP", "Other"), original });

	ASSERT_EQ(ShikigamiDataOutcome::SUCCESS, ShikigamiRepository::updateShikigami(path.string(), original, updated));
	std::vector<Shikigami> actual = loadCsv(path);
	ASSERT_EQ(2u, actual.size());
	TEST_ASSERT_EQUAL("Other", actual[0].Name);
	assertShikigami(updated, actual[1]);
}

void ShikigamiRepository_Test::updateShikigami_rejectsMissingTarget()
{
	TemporaryDirectory directory;
	std::filesystem::path path = directory.getPath() / "ShikigamiData.csv";
	writeCsv(path, { createShikigami("SSR", "Existing") });
	std::string original = TestFileUtil::readTextFile(path);

	ShikigamiDataOutcome outcome = ShikigamiRepository::updateShikigami(path.string(), createShikigami("SSR", "Missing"), createShikigami("SSR", "New"));

	TEST_ASSERT_EQUAL(ShikigamiDataOutcome::NOT_FOUND, outcome);
	TEST_ASSERT_EQUAL(original, TestFileUtil::readTextFile(path));
}

void ShikigamiRepository_Test::updateShikigami_rejectsDuplicateOtherThanSelf()
{
	TemporaryDirectory directory;
	std::filesystem::path path = directory.getPath() / "ShikigamiData.csv";
	Shikigami target = createShikigami("SSR", "Original");
	writeCsv(path, { target, createShikigami("SSR", "Occupied") });
	std::string original = TestFileUtil::readTextFile(path);

	ShikigamiDataOutcome outcome = ShikigamiRepository::updateShikigami(path.string(), target, createShikigami("SSR", "Occupied"));

	TEST_ASSERT_EQUAL(ShikigamiDataOutcome::DUPLICATE, outcome);
	TEST_ASSERT_EQUAL(original, TestFileUtil::readTextFile(path));
}

void ShikigamiRepository_Test::updateShikigami_allowsUpdatingSameIdentity()
{
	TemporaryDirectory directory;
	std::filesystem::path path = directory.getPath() / "ShikigamiData.csv";
	Shikigami original = createShikigami("SSR", "SameName", 100.0);
	Shikigami updated = createShikigami("SSR", "SameName", 550.0);
	writeCsv(path, { original });

	ASSERT_EQ(ShikigamiDataOutcome::SUCCESS, ShikigamiRepository::updateShikigami(path.string(), original, updated));
	std::vector<Shikigami> actual = loadCsv(path);
	ASSERT_EQ(1u, actual.size());
	assertShikigami(updated, actual[0]);
}

void ShikigamiRepository_Test::getRecoveryCandidateShikigamiList_excludesExistingAndInvalidRecords()
{
	TemporaryDirectory directory;
	std::filesystem::path current = directory.getPath() / "Current.csv";
	std::filesystem::path source = directory.getPath() / "Source.csv";
	Shikigami existing = createShikigami("SP", "Existing");
	Shikigami missing = createShikigami("SSR", "Recoverable");
	writeCsv(current, { existing });
	TestFileUtil::writeTextFile(source, CSV_HEADER + "\n" + ShikigamiCsvConverter::toCsvLine(existing) + "\nBrokenRow\n" + ShikigamiCsvConverter::toCsvLine(missing) + "\n");

	std::vector<Shikigami> candidates;
	ShikigamiDataOutcome outcome = ShikigamiRepository::getRecoveryCandidateShikigamiList(current.string(), source.string(), candidates);

	ASSERT_EQ(ShikigamiDataOutcome::SUCCESS, outcome);
	ASSERT_EQ(1u, candidates.size());
	assertShikigami(missing, candidates[0]);
}

void ShikigamiRepository_Test::getRecoveryCandidateShikigamiList_failsWhenSourceIsMissing()
{
	TemporaryDirectory directory;
	std::filesystem::path current = directory.getPath() / "Current.csv";
	std::filesystem::path source = directory.getPath() / "Missing.csv";
	writeCsv(current, { createShikigami("SP", "Existing") });

	std::vector<Shikigami> candidates;
	TEST_ASSERT_EQUAL(ShikigamiDataOutcome::FILE_OPEN_FAILED, ShikigamiRepository::getRecoveryCandidateShikigamiList(current.string(), source.string(), candidates));
	TEST_ASSERT_TRUE(candidates.empty());
}

void ShikigamiRepository_Test::recoverShikigamiList_insertsSelectedCandidateInSourceOrder()
{
	TemporaryDirectory directory;
	std::filesystem::path current = directory.getPath() / "Current.csv";
	std::filesystem::path source = directory.getPath() / "Source.csv";
	Shikigami first = createShikigami("SP", "First");
	Shikigami restored = createShikigami("SSR", "Restored");
	Shikigami third = createShikigami("SSR", "Third");
	Shikigami fourth = createShikigami("SR", "Fourth");
	writeCsv(current, { first, third, fourth });
	writeCsv(source, { first, restored, third, fourth });

	std::vector<Shikigami> candidates;
	ASSERT_EQ(ShikigamiDataOutcome::SUCCESS, ShikigamiRepository::getRecoveryCandidateShikigamiList(current.string(), source.string(), candidates));
	ASSERT_EQ(1u, candidates.size());
	ASSERT_EQ(ShikigamiDataOutcome::SUCCESS, ShikigamiRepository::recoverShikigamiList(current.string(), candidates));

	std::vector<Shikigami> actual = loadCsv(current);
	ASSERT_EQ(4u, actual.size());
	TEST_ASSERT_EQUAL("First", actual[0].Name);
	TEST_ASSERT_EQUAL("Restored", actual[1].Name);
	TEST_ASSERT_EQUAL("Third", actual[2].Name);
	TEST_ASSERT_EQUAL("Fourth", actual[3].Name);
}

void ShikigamiRepository_Test::recoverShikigamiList_doesNotRestoreUnselectedCandidates()
{
	TemporaryDirectory directory;
	std::filesystem::path current = directory.getPath() / "Current.csv";
	std::filesystem::path source = directory.getPath() / "Source.csv";
	Shikigami existing = createShikigami("SP", "Existing");
	Shikigami selected = createShikigami("SSR", "Selected");
	Shikigami unselected = createShikigami("SR", "Unselected");
	writeCsv(current, { existing });
	writeCsv(source, { existing, selected, unselected });

	std::vector<Shikigami> candidates;
	ASSERT_EQ(ShikigamiDataOutcome::SUCCESS, ShikigamiRepository::getRecoveryCandidateShikigamiList(current.string(), source.string(), candidates));
	ASSERT_EQ(2u, candidates.size());
	ASSERT_EQ(ShikigamiDataOutcome::SUCCESS, ShikigamiRepository::recoverShikigamiList(current.string(), { selected }));

	std::vector<Shikigami> actual = loadCsv(current);
	ASSERT_EQ(2u, actual.size());
	TEST_ASSERT_EQUAL("Existing", actual[0].Name);
	TEST_ASSERT_EQUAL("Selected", actual[1].Name);
}

void ShikigamiRepository_Test::recoverShikigamiList_doesNotChangeCsvWhenNothingIsSelected()
{
	TemporaryDirectory directory;
	std::filesystem::path current = directory.getPath() / "Current.csv";
	std::filesystem::path source = directory.getPath() / "Source.csv";
	writeCsv(current, { createShikigami("SP", "Existing") });
	writeCsv(source, { createShikigami("SP", "Existing"), createShikigami("SSR", "Candidate") });
	std::string original = TestFileUtil::readTextFile(current);

	std::vector<Shikigami> candidates;
	ASSERT_EQ(ShikigamiDataOutcome::SUCCESS, ShikigamiRepository::getRecoveryCandidateShikigamiList(current.string(), source.string(), candidates));
	ASSERT_EQ(1u, candidates.size());
	TEST_ASSERT_EQUAL(ShikigamiDataOutcome::SUCCESS, ShikigamiRepository::recoverShikigamiList(current.string(), {}));
	TEST_ASSERT_EQUAL(original, TestFileUtil::readTextFile(current));
}
