using SaveData.Model;

namespace SaveDataTest.TestCommon
{
	public static class TestAssert
	{
		public static void AreEqual(StatValueSaveData expected, StatValueSaveData actual)
		{
			if (assertNull(expected, actual))
			{
				return;
			}

			Assert.AreEqual(expected.Type, actual.Type);
			Assert.AreEqual(expected.Value, actual.Value);
		}

		public static void AreEqual(MitamaSaveData expected, MitamaSaveData actual)
		{
			if (assertNull(expected, actual))
			{
				return;
			}

			Assert.AreEqual(expected.Slot, actual.Slot);
			AreEqual(expected.MainStat, actual.MainStat);
			assertListEqual(expected.SubStats, actual.SubStats, AreEqual);
		}

		public static void AreEqual(SetEffectSaveData expected, SetEffectSaveData actual)
		{
			if (assertNull(expected, actual))
			{
				return;
			}

			AreEqual(expected.Stat, actual.Stat);
		}

		public static void AreEqual(MitamaSetSaveData expected, MitamaSetSaveData actual)
		{
			if (assertNull(expected, actual))
			{
				return;
			}

			assertListEqual(expected.Mitamas, actual.Mitamas, AreEqual);
			assertListEqual(expected.SetEffects, actual.SetEffects, AreEqual);
			assertListEqual(expected.UniqueEffects, actual.UniqueEffects, AreEqual);
		}

		public static void AreEqual(StatusSaveData expected, StatusSaveData actual)
		{
			if (assertNull(expected, actual))
			{
				return;
			}

			Assert.AreEqual(expected.Attack, actual.Attack);
			Assert.AreEqual(expected.HP, actual.HP);
			Assert.AreEqual(expected.Defense, actual.Defense);
			Assert.AreEqual(expected.Speed, actual.Speed);
			Assert.AreEqual(expected.CritRate, actual.CritRate);
			Assert.AreEqual(expected.CritDamage, actual.CritDamage);
			Assert.AreEqual(expected.EffectHit, actual.EffectHit);
			Assert.AreEqual(expected.EffectResist, actual.EffectResist);
		}

		public static void AreEqual(BuildSaveData expected, BuildSaveData actual)
		{
			if (assertNull(expected, actual))
			{
				return;
			}

			Assert.AreEqual(expected.ShikigamiName, actual.ShikigamiName);
			AreEqual(expected.MitamaSet, actual.MitamaSet);
		}

		public static void AreEqual(CalculationSnapshotSaveData expected, CalculationSnapshotSaveData actual)
		{
			if (assertNull(expected, actual))
			{
				return;
			}

			Assert.AreEqual(expected.SnapshotName, actual.SnapshotName);
			Assert.AreEqual(expected.CreatedAt, actual.CreatedAt);
			Assert.AreEqual(expected.ShikigamiName, actual.ShikigamiName);
			AreEqual(expected.MitamaSet, actual.MitamaSet);
			AreEqual(expected.MitamaStatus, actual.MitamaStatus);
			AreEqual(expected.FinalStatus, actual.FinalStatus);
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
