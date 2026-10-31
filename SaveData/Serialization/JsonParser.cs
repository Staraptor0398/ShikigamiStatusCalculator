using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SaveData.Serialization
{
	internal sealed class JsonParser
	{
		private readonly string mSource;
		private int mPosition;

		private JsonParser(string source)
		{
			mSource = source ?? throw new ArgumentNullException(nameof(source));
		}

		public static object Parse(string source)
		{
			JsonParser reader = new JsonParser(source);
			object value = reader.readValue(0);

			reader.skipSpace();

			if (reader.mPosition != source.Length)
			{
				throw new FormatException("Unexpected text after JSON value.");
			}

			return value;
		}

		private object readValue(int depth)
		{
			if (depth > 128)
			{
				throw new FormatException("JSON nesting exceeds 128 levels.");
			}

			skipSpace();

			if (mPosition >= mSource.Length)
			{
				throw new FormatException("Unexpected end of JSON.");
			}

			char c = mSource[mPosition];

			if (c == '{')
			{
				return readObject(depth + 1);
			}

			if (c == '[')
			{
				return readArray(depth + 1);
			}

			if (c == '"')
			{
				return readString();
			}

			if (c == 't')
			{
				require("true");
				return true;
			}

			if (c == 'f')
			{
				require("false");
				return false;
			}

			if (c == 'n')
			{
				require("null");
				return null;
			}

			if (c == '-' || (c >= '0' && c <= '9'))
			{
				return readNumber();
			}

			throw new FormatException("Invalid JSON token at index " + mPosition);
		}

		private Dictionary<string, object> readObject(int depth)
		{
			expect('{');

			Dictionary<string, object> result = new Dictionary<string, object>(StringComparer.Ordinal);

			skipSpace();

			if (consume('}'))
			{
				return result;
			}

			while (true)
			{
				skipSpace();

				if (mPosition >= mSource.Length || mSource[mPosition] != '"')
				{
					throw new FormatException("JSON object key must be a string.");
				}

				string name = readString();

				skipSpace();
				expect(':');

				result[name] = readValue(depth);

				skipSpace();

				if (consume('}'))
				{
					return result;
				}

				expect(',');
			}
		}

		private List<object> readArray(int depth)
		{
			expect('[');

			List<object> result = new List<object>();

			skipSpace();

			if (consume(']'))
			{
				return result;
			}

			while (true)
			{
				result.Add(readValue(depth));

				skipSpace();

				if (consume(']'))
				{
					return result;
				}

				expect(',');
			}
		}

		private string readString()
		{
			expect('"');

			StringBuilder value = new StringBuilder();

			while (mPosition < mSource.Length)
			{
				char c = mSource[mPosition++];

				if (c == '"')
				{
					return value.ToString();
				}

				if (c < 0x20)
				{
					throw new FormatException("Unescaped JSON control character.");
				}

				if (c != '\\')
				{
					appendRawCharacter(value, c);
					continue;
				}

				if (mPosition >= mSource.Length)
				{
					throw new FormatException("Incomplete JSON escape.");
				}

				c = mSource[mPosition++];

				switch (c)
				{
					case '"':
						value.Append('"');
						break;

					case '\\':
						value.Append('\\');
						break;

					case '/':
						value.Append('/');
						break;

					case 'b':
						value.Append('\b');
						break;

					case 'f':
						value.Append('\f');
						break;

					case 'n':
						value.Append('\n');
						break;

					case 'r':
						value.Append('\r');
						break;

					case 't':
						value.Append('\t');
						break;

					case 'u':
						appendUnicodeEscape(value);
						break;

					default:
						throw new FormatException("Unknown JSON escape.");
				}
			}

			throw new FormatException("Unterminated JSON string.");
		}

		private void appendRawCharacter(StringBuilder value, char c)
		{
			if (char.IsHighSurrogate(c))
			{
				if (mPosition >= mSource.Length || !char.IsLowSurrogate(mSource[mPosition]))
				{
					throw new FormatException("Unpaired high surrogate.");
				}

				value.Append(c);
				value.Append(mSource[mPosition++]);
				return;
			}

			if (char.IsLowSurrogate(c))
			{
				throw new FormatException("Unpaired low surrogate.");
			}

			value.Append(c);
		}

		private void appendUnicodeEscape(StringBuilder value)
		{
			char first = readUnicodeCharacter();

			if (char.IsHighSurrogate(first))
			{
				if (mPosition + 2 > mSource.Length || mSource[mPosition] != '\\' || mSource[mPosition + 1] != 'u')
				{
					throw new FormatException("Unpaired high surrogate.");
				}

				mPosition += 2;

				char second = readUnicodeCharacter();

				if (!char.IsLowSurrogate(second))
				{
					throw new FormatException("Invalid Unicode surrogate pair.");
				}

				value.Append(first);
				value.Append(second);
				return;
			}

			if (char.IsLowSurrogate(first))
			{
				throw new FormatException("Unpaired low surrogate.");
			}

			value.Append(first);
		}

		private char readUnicodeCharacter()
		{
			if (mPosition + 4 > mSource.Length)
			{
				throw new FormatException("Incomplete Unicode escape.");
			}

			string hex = mSource.Substring(mPosition, 4);

			if (!ushort.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ushort code))
			{
				throw new FormatException("Invalid Unicode escape.");
			}

			mPosition += 4;

			return (char)code;
		}

		private object readNumber()
		{
			int begin = mPosition;

			consume('-');

			if (consume('0'))
			{
				if (mPosition < mSource.Length && mSource[mPosition] >= '0' && mSource[mPosition] <= '9')
				{
					throw new FormatException("Invalid leading zero.");
				}
			}
			else
			{
				if (mPosition >= mSource.Length || mSource[mPosition] < '1' || mSource[mPosition] > '9')
				{
					throw new FormatException("Invalid JSON number.");
				}

				while (mPosition < mSource.Length && mSource[mPosition] >= '0' && mSource[mPosition] <= '9')
				{
					mPosition++;
				}
			}

			if (consume('.'))
			{
				int digits = mPosition;

				while (mPosition < mSource.Length && mSource[mPosition] >= '0' && mSource[mPosition] <= '9')
				{
					mPosition++;
				}

				if (digits == mPosition)
				{
					throw new FormatException("Missing digits after decimal point.");
				}
			}

			if (consume('e') || consume('E'))
			{
				if (!consume('+'))
				{
					consume('-');
				}

				int digits = mPosition;

				while (mPosition < mSource.Length && mSource[mPosition] >= '0' && mSource[mPosition] <= '9')
				{
					mPosition++;
				}

				if (digits == mPosition)
				{
					throw new FormatException("Missing exponent digits.");
				}
			}

			string number = mSource.Substring(begin, mPosition - begin);

			if (!double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out double result) || double.IsInfinity(result) || double.IsNaN(result))
			{
				throw new FormatException("Invalid JSON number.");
			}

			return result;
		}

		private void require(string literal)
		{
			if (mPosition + literal.Length > mSource.Length || string.CompareOrdinal(mSource, mPosition, literal, 0, literal.Length) != 0)
			{
				throw new FormatException("Invalid JSON literal.");
			}

			mPosition += literal.Length;
		}

		private void skipSpace()
		{
			while (mPosition < mSource.Length)
			{
				char c = mSource[mPosition];

				if (c != ' ' && c != '\n' && c != '\r' && c != '\t')
				{
					return;
				}

				mPosition++;
			}
		}

		private bool consume(char c)
		{
			if (mPosition >= mSource.Length || mSource[mPosition] != c)
			{
				return false;
			}

			mPosition++;

			return true;
		}

		private void expect(char c)
		{
			skipSpace();

			if (!consume(c))
			{
				throw new FormatException("Expected '" + c + "' at index " + mPosition);
			}
		}
	}
}
