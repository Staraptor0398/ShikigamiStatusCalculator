#pragma once

#include<string>

using namespace System;

// std::string と C# String の相互変換を行うクラス
class StringConverter
{
public:
	// UTF-8 std::string -> C# String
	static String^ toManagedString(const std::string& text);

	// C# String -> Windowsに渡す用 std::string
	static std::string toStdString(String^ text);

	// C# String -> ファイル書き込みデータ用 std::string
	static std::string toUtf8String(String^ text);
};
