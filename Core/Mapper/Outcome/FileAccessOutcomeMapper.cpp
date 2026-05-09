#include "pch.h"
#include "FileAccessOutcomeMapper.h"

ShikigamiDataOutcome FileAccessOutcomeMapper::toShikigamiDataOutcome(FileAccessOutcome fileAccessOutcome)
{
	switch (fileAccessOutcome) {
		case FileAccessOutcome::SUCCESS:				return ShikigamiDataOutcome::SUCCESS;
		case FileAccessOutcome::FILE_OPEN_FAILED:		return ShikigamiDataOutcome::FILE_OPEN_FAILED;
		case FileAccessOutcome::FILE_READ_FAILED:		return ShikigamiDataOutcome::FILE_READ_FAILED;
		case FileAccessOutcome::FILE_WRITE_FAILED:		return ShikigamiDataOutcome::FILE_WRITE_FAILED;
		case FileAccessOutcome::INVALID_FORMAT:			return ShikigamiDataOutcome::INVALID_FORMAT;
		case FileAccessOutcome::UNKNOWN_ERROR:
		default:										return ShikigamiDataOutcome::UNKNOWN_ERROR;
	}
}