#pragma once

class FileAccess_Test
{
public:
	static void saveShikigami_replacesExistingCsv();
	static void saveShikigami_preservesDataAfterReload();
	static void saveShikigami_preservesOriginalWhenReplacementFails();
	static void saveShikigami_failsWhenTemporaryDirectoryDoesNotExist();
	static void saveShikigami_failsWhenDestinationDoesNotExist();

	static void loadShikigami_readsValidCsv();
	static void loadShikigami_rejectsInvalidHeader();
	static void loadShikigami_rejectsInvalidRecord();
	static void loadShikigami_failsWhenFileDoesNotExist();

	static void loadValidShikigami_skipsInvalidRecords();
};
