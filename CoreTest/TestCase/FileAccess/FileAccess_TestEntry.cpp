#include "pch.h"

#include <gtest/gtest.h>

#include "FileAccess_Test.h"

#define FILE_ACCESS_TEST_LIST \
	X(T001_saveShikigami_replacesExistingCsv, saveShikigami_replacesExistingCsv) \
	X(T002_saveShikigami_preservesDataAfterReload, saveShikigami_preservesDataAfterReload) \
	X(T003_saveShikigami_preservesOriginalWhenReplacementFails, saveShikigami_preservesOriginalWhenReplacementFails) \
	X(T004_saveShikigami_failsWhenTemporaryDirectoryDoesNotExist, saveShikigami_failsWhenTemporaryDirectoryDoesNotExist) \
	X(T005_saveShikigami_failsWhenDestinationDoesNotExist, saveShikigami_failsWhenDestinationDoesNotExist) \
	X(T006_loadShikigami_readsValidCsv, loadShikigami_readsValidCsv) \
	X(T007_loadShikigami_rejectsInvalidHeader, loadShikigami_rejectsInvalidHeader) \
	X(T008_loadShikigami_rejectsInvalidRecord, loadShikigami_rejectsInvalidRecord) \
	X(T009_loadShikigami_failsWhenFileDoesNotExist, loadShikigami_failsWhenFileDoesNotExist) \
	X(T010_loadValidShikigami_skipsInvalidRecords, loadValidShikigami_skipsInvalidRecords)

#define X(testName, functionName) \
	TEST(FileAccess_Test, testName) \
	{ \
		FileAccess_Test::functionName(); \
	}

FILE_ACCESS_TEST_LIST

#undef X
#undef FILE_ACCESS_TEST_LIST
