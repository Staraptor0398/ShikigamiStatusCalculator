#pragma once

class ShikigamiCsvConverter_Test
{
public:
	static void toShikigami_parsesValidCsv();
	static void toShikigami_parsesJapaneseName();
	static void toShikigami_parsesFractionalAndNegativeValues();
	static void toShikigami_rejectsMissingColumns();
	static void toShikigami_rejectsExtraColumns();
	static void toShikigami_rejectsInvalidNumericValue();
	static void toShikigami_rejectsOutOfRangeNumericValue();
	static void toShikigami_acceptsEmptyName();
	static void toCsvLine_serializesAllColumns();
	static void toCsvLine_preservesColumnOrder();
	static void roundTrip_preservesShikigamiData();
};
