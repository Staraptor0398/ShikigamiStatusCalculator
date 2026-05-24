#pragma once

#include "../../Outcome/FileAccessOutcome.h"
#include "../../Outcome/ShikigamiDataOutcome.h"

class FileAccessOutcomeMapper
{
public:
	static ShikigamiDataOutcome toShikigamiDataOutcome(FileAccessOutcome fileAccessOutcome);
};
