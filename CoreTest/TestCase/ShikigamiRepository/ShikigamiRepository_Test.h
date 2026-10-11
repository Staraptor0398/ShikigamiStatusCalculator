#pragma once

class ShikigamiRepository_Test
{
public:
	static void getShikigamiList_readsExistingData();
	static void getShikigamiList_failsWhenFileIsMissing();
	static void addShikigami_insertsInRarityOrder();
	static void addShikigami_rejectsDuplicateRarityAndName();
	static void addShikigami_allowsSameNameWithDifferentRarity();
	static void addShikigami_preservesInvalidCsv();
	static void updateShikigami_updatesExistingData();
	static void updateShikigami_rejectsMissingTarget();
	static void updateShikigami_rejectsDuplicateOtherThanSelf();
	static void updateShikigami_allowsUpdatingSameIdentity();
	static void getRecoveryCandidateShikigamiList_excludesExistingAndInvalidRecords();
	static void getRecoveryCandidateShikigamiList_failsWhenSourceIsMissing();
	static void recoverShikigamiList_insertsSelectedCandidateInSourceOrder();
	static void recoverShikigamiList_doesNotRestoreUnselectedCandidates();
	static void recoverShikigamiList_doesNotChangeCsvWhenNothingIsSelected();
};
