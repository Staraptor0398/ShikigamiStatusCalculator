using SaveData.Access;
using SaveData.Model;
using SaveDataTest.TestCommon;

namespace SaveDataTest.TestCase
{
	[TestClass]
	public class JsonDataAccess_Test
	{
		[TestMethod]
		public void Deserialize_ValidStatValue()
		{
			string json = "{\"Type\":\"攻撃力\",\"Value\":486.0}";

			StatValueSaveData actual = JsonDataAccess.Deserialize<StatValueSaveData>(json);

			Assert.AreEqual("攻撃力", actual.Type);
			Assert.AreEqual(486.0, actual.Value);
		}

		[TestMethod]
		public void Deserialize_EscapedCharacters()
		{
			string json = "{\"Type\":\"A\\\"B\\\\C\\nD\\tE\\u3042\",\"Value\":1.0}";

			StatValueSaveData actual = JsonDataAccess.Deserialize<StatValueSaveData>(json);

			Assert.AreEqual("A\"B\\C\nD\tEあ", actual.Type);
		}

		[TestMethod]
		public void Deserialize_UnicodeSurrogatePair()
		{
			string json = "{\"Type\":\"\\uD83D\\uDE00\",\"Value\":1.0}";

			StatValueSaveData actual = JsonDataAccess.Deserialize<StatValueSaveData>(json);

			Assert.AreEqual("\uD83D\uDE00", actual.Type);
		}

		[TestMethod]
		public void Deserialize_UnpairedHighSurrogate()
		{
			string json = "{\"Type\":\"\\uD83D\",\"Value\":1.0}";

			assertInvalidJson(json);
		}

		[TestMethod]
		public void Deserialize_UnpairedLowSurrogate()
		{
			string json = "{\"Type\":\"\\uDE00\",\"Value\":1.0}";

			assertInvalidJson(json);
		}

		[TestMethod]
		public void Deserialize_InvalidUnicodeEscape()
		{
			string json = "{\"Type\":\"\\uZZZZ\",\"Value\":1.0}";

			assertInvalidJson(json);
		}

		[TestMethod]
		public void Deserialize_UnescapedControlCharacter()
		{
			string json = "{\"Type\":\"A\nB\",\"Value\":1.0}";

			assertInvalidJson(json);
		}

		[TestMethod]
		public void Deserialize_InvalidTrailingComma()
		{
			string json = "{\"Type\":\"攻撃力\",\"Value\":1.0,}";

			assertInvalidJson(json);
		}

		[TestMethod]
		public void Deserialize_InvalidTrailingText()
		{
			string json = "{\"Type\":\"攻撃力\",\"Value\":1.0} extra";

			assertInvalidJson(json);
		}

		[DataRow("01")]
		[DataRow("1.")]
		[DataRow("1e")]
		[DataRow("-")]
		[DataRow("+1")]
		[DataRow("NaN")]
		[DataRow("Infinity")]
		[DataTestMethod]
		public void Deserialize_InvalidNumber(string value)
		{
			string json = "{\"Type\":\"攻撃力\",\"Value\":" + value + "}";

			assertInvalidJson(json);
		}

		[TestMethod]
		public void Deserialize_NumberExponent()
		{
			string json = "{\"Type\":\"攻撃力\",\"Value\":1.25e2}";

			StatValueSaveData actual = JsonDataAccess.Deserialize<StatValueSaveData>(json);

			Assert.AreEqual(125.0, actual.Value);
		}

		[TestMethod]
		public void Deserialize_Int32Maximum()
		{
			string json = "{\"Slot\":2147483647}";

			MitamaSaveData actual = JsonDataAccess.Deserialize<MitamaSaveData>(json);

			Assert.AreEqual(int.MaxValue, actual.Slot);
		}

		[TestMethod]
		public void Deserialize_Int32Overflow()
		{
			string json = "{\"Slot\":2147483648}";

			assertInvalidJson<MitamaSaveData>(json);
		}

		[TestMethod]
		public void Deserialize_Int32Fraction()
		{
			string json = "{\"Slot\":1.5}";

			assertInvalidJson<MitamaSaveData>(json);
		}

		[TestMethod]
		public void Deserialize_InvalidNumericType()
		{
			string json = "{\"Type\":\"攻撃力\",\"Value\":\"abc\"}";

			assertInvalidJson(json);
		}

		[TestMethod]
		public void Deserialize_NullNumericValue()
		{
			string json = "{\"Type\":\"攻撃力\",\"Value\":null}";

			assertInvalidJson(json);
		}

		[TestMethod]
		public void Deserialize_WrongObjectType()
		{
			string json = "{\"Type\":\"攻撃力\",\"Value\":[]}";

			assertInvalidJson(json);
		}

		[TestMethod]
		public void Deserialize_UnknownProperty()
		{
			string json = "{\"Type\":\"攻撃力\",\"Value\":486.0,\"Unknown\":123}";

			StatValueSaveData actual = JsonDataAccess.Deserialize<StatValueSaveData>(json);

			Assert.AreEqual("攻撃力", actual.Type);
			Assert.AreEqual(486.0, actual.Value);
		}

		[TestMethod]
		public void Deserialize_CaseSensitiveProperty()
		{
			string json = "{\"Type\":\"攻撃力\",\"value\":486.0}";

			StatValueSaveData actual = JsonDataAccess.Deserialize<StatValueSaveData>(json);

			Assert.AreEqual("攻撃力", actual.Type);
			Assert.AreEqual(0.0, actual.Value);
		}

		[TestMethod]
		public void Deserialize_LegacyDefense()
		{
			string json = "{\"Deffense\":300.0}";

			StatusSaveData actual = JsonDataAccess.Deserialize<StatusSaveData>(json);

			Assert.AreEqual(300.0, actual.Defense);
		}

		[TestMethod]
		public void Deserialize_NestedArray()
		{
			string json = "{\"Mitamas\":[],\"SetEffects\":[],\"UniqueEffects\":[]}";

			MitamaSetSaveData actual = JsonDataAccess.Deserialize<MitamaSetSaveData>(json);

			Assert.IsNotNull(actual.Mitamas);
			Assert.IsNotNull(actual.SetEffects);
			Assert.IsNotNull(actual.UniqueEffects);
			Assert.AreEqual(0, actual.Mitamas.Count);
		}

		[TestMethod]
		public void Deserialize_ExcessiveNesting()
		{
			string json = new string('[', 130) + "0" + new string(']', 130);

			assertInvalidJson(json);
		}

		[TestMethod]
		public void SaveLoad_EscapedCharacters()
		{
			using (var directory = new TemporaryTestDirectory())
			{
				string path = directory.GetFilePath("Escaped.json");

				StatValueSaveData expected = new StatValueSaveData
				{
					Type = "攻撃力\"\\\n\tあ😀",
					Value = 486.0
				};

				JsonDataAccess.Save(path, expected);

				StatValueSaveData actual = JsonDataAccess.Load<StatValueSaveData>(path);

				Assert.AreEqual(expected.Type, actual.Type);
				Assert.AreEqual(expected.Value, actual.Value);
			}
		}

		[TestMethod]
		public void Save_InvalidNumericValue()
		{
			using (var directory = new TemporaryTestDirectory())
			{
				string path = directory.GetFilePath("Invalid.json");

				StatValueSaveData data = new StatValueSaveData
				{
					Type = "攻撃力",
					Value = double.NaN
				};

				Assert.ThrowsExactly<InvalidOperationException>(() => JsonDataAccess.Save(path, data));
			}
		}

		[TestMethod]
		public void Save_UnpairedSurrogate()
		{
			using (var directory = new TemporaryTestDirectory())
			{
				string path = directory.GetFilePath("InvalidUnicode.json");

				StatValueSaveData data = new StatValueSaveData
				{
					Type = "\uD83D",
					Value = 1.0
				};

				Assert.ThrowsExactly<ArgumentException>(() => JsonDataAccess.Save(path, data));
			}
		}

		private static void assertInvalidJson<T>(string json)
		{
			bool exceptionThrown = false;

			try
			{
				JsonDataAccess.Deserialize<T>(json);
			}
			catch (Exception ex) when (
				ex is FormatException ||
				ex is ArgumentException ||
				ex is InvalidCastException ||
				ex is OverflowException)
			{
				exceptionThrown = true;
			}

			Assert.IsTrue(exceptionThrown, "Invalid JSON or incompatible value was accepted.");
		}

		private static void assertInvalidJson(string json)
		{
			assertInvalidJson<StatValueSaveData>(json);
		}
	}
}
