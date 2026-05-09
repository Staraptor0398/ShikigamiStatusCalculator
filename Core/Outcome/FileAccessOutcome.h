#pragma once

enum class FileAccessOutcome
{
	SUCCESS,
	FILE_OPEN_FAILED,
	FILE_READ_FAILED,
	FILE_WRITE_FAILED,
	INVALID_FORMAT,
	UNKNOWN_ERROR
};