#pragma once

#include<string>

using namespace System;

class StringConverter
{
public:
	// UTF-8 std::string -> C# string
	static String^ toManagedString(const std::string& text);

	// C# string -> Windowsに渡す用 std::string
	static std::string toStdString(String^ text);

	// C# string -> ファイル書き込みデータ用 std::string
	static std::string toUtf8String(String^ text);
};
