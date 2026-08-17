using GuiTest.TestCommon.Model;

namespace GuiTest.TestCommon
{
	public static class TestAssert
	{
		public static void AreEqual(StatValueTestData expected, StatValueTestData actual)
		{
			if (assertNull(expected, actual))
			{
				return;
			}

			Assert.AreEqual(expected.Type, actual.Type);
			Assert.AreEqual(expected.Value, actual.Value);
		}

		public static void AreEqual(MitamaTestData expected, MitamaTestData actual)
		{
			if (assertNull(expected, actual))
			{
				return;
			}

			AreEqual(expected.MainStat, actual.MainStat);
			assertListEqual(expected.SubStat, actual.SubStat, AreEqual);
		}

		public static void AreEqual(SetEffectTestData expected, SetEffectTestData actual)
		{
			if (assertNull(expected, actual))
			{
				return;
			}

			AreEqual(expected.Stat, actual.Stat);
		}

		public static void AreEqual(MitamaSetTestData expected, MitamaSetTestData actual)
		{
			if (assertNull(expected, actual))
			{
				return;
			}

			assertListEqual(expected.Mitamas, actual.Mitamas, AreEqual);
			assertListEqual(expected.SetEffects, actual.SetEffects, AreEqual);
			assertListEqual(expected.UniqueEffects, actual.UniqueEffects, AreEqual);
		}

		private static bool assertNull<T>(T expected, T actual) where T : class
		{
			if (expected != null && actual != null)
			{
				return false;
			}

			Assert.AreEqual(expected, actual);
			return true;
		}

		private static void assertListEqual<T>(IList<T> expected, IList<T> actual, Action<T, T> assertElement)
		{
			if (expected == null || actual == null)
			{
				Assert.AreEqual(expected, actual);
				return;
			}

			Assert.AreEqual(expected.Count, actual.Count);

			for (int i = 0; i < expected.Count; i++)
			{
				assertElement(expected[i], actual[i]);
			}
		}
	}
}
