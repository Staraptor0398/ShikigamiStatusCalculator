
using Gui.Function;
using SaveData.Access;
using SaveData.Model;

namespace GuiTest.TestCase
{
	[TestClass]
	public class SnapshotComparisonFunction_Test
	{
		[TestMethod]
		public void Success()
		{
			using (var fixture = new SnapshotFixture())
			{
				var function = new SnapshotComparisonFunction();

				string baseSnapshotName;
				string targetSnapshotName;
				StatusComparisonResultDto comparisonResult;

				SnapshotComparisonOutcome outcome = function.TryCompare(fixture.BaseFilePath, fixture.TargetFilePath, out baseSnapshotName, out targetSnapshotName, out comparisonResult);

				Assert.AreEqual(SnapshotComparisonOutcome.SUCCESS, outcome);
				Assert.AreEqual("Base", baseSnapshotName);
				Assert.AreEqual("Target", targetSnapshotName);

				Assert.IsNotNull(comparisonResult);

				Assert.AreEqual(300.0, comparisonResult.AttackDifference);
				Assert.AreEqual(-1000.0, comparisonResult.HpDifference);
				Assert.AreEqual(50.0, comparisonResult.DefenseDifference);
				Assert.AreEqual(15.0, comparisonResult.SpeedDifference);
				Assert.AreEqual(15.0, comparisonResult.CriticalRateDifference);
				Assert.AreEqual(-10.0, comparisonResult.CriticalDamageDifference);
				Assert.AreEqual(0.0, comparisonResult.EffectHitDifference);
				Assert.AreEqual(-5.0, comparisonResult.EffectResistDifference);
			}
		}

		[TestMethod]
		public void BaseSnapshotNull()
		{
			using (var fixture = new SnapshotFixture())
			{
				fixture.SaveBase(null);

				assertOutcome(fixture, SnapshotComparisonOutcome.SNAPSHOT_NULL);
			}
		}

		[TestMethod]
		public void TargetSnapshotNull()
		{
			using (var fixture = new SnapshotFixture())
			{
				fixture.SaveTarget(null);

				assertOutcome(fixture, SnapshotComparisonOutcome.SNAPSHOT_NULL);
			}
		}

		[TestMethod]
		public void BaseStatusNull()
		{
			using (var fixture = new SnapshotFixture())
			{
				fixture.SaveBase(createSnapshot("Base", null));

				assertOutcome(fixture, SnapshotComparisonOutcome.STATUS_NULL);
			}
		}

		[TestMethod]
		public void TargetStatusNull()
		{
			using (var fixture = new SnapshotFixture())
			{
				fixture.SaveTarget(createSnapshot("Target", null));

				assertOutcome(fixture, SnapshotComparisonOutcome.STATUS_NULL);
			}
		}

		[TestMethod]
		public void TargetFileNotFound()
		{
			using (var fixture = new SnapshotFixture())
			{
				File.Delete(fixture.TargetFilePath);

				var function = new SnapshotComparisonFunction();

				string baseSnapshotName = null;
				string targetSnapshotName = null;
				StatusComparisonResultDto comparisonResult = null;

				bool exceptionThrown = false;

				try
				{
					function.TryCompare(fixture.BaseFilePath, fixture.TargetFilePath, out baseSnapshotName, out targetSnapshotName, out comparisonResult);
				}
				catch (FileNotFoundException)
				{
					exceptionThrown = true;
				}

				Assert.IsTrue(exceptionThrown);

				Assert.IsNull(baseSnapshotName);
				Assert.IsNull(targetSnapshotName);
				Assert.IsNull(comparisonResult);
			}
		}

		private static void assertOutcome(SnapshotFixture fixture, SnapshotComparisonOutcome expected)
		{
			var function = new SnapshotComparisonFunction();

			string baseSnapshotName;
			string targetSnapshotName;
			StatusComparisonResultDto comparisonResult;

			SnapshotComparisonOutcome actual = function.TryCompare(fixture.BaseFilePath, fixture.TargetFilePath, out baseSnapshotName, out targetSnapshotName, out comparisonResult);

			Assert.AreEqual(expected, actual);

			Assert.IsNull(baseSnapshotName);
			Assert.IsNull(targetSnapshotName);
			Assert.IsNull(comparisonResult);
		}

		private static CalculationSnapshotSaveData createSnapshot(string name, StatusSaveData finalStatus)
		{
			return new CalculationSnapshotSaveData
			{
				SnapshotName = name,
				CreatedAt = "2026-11-19 00:00:00",
				ShikigamiName = "Test",
				MitamaSet = new MitamaSetSaveData(),
				MitamaStatus = new StatusSaveData(),
				FinalStatus = finalStatus
			};
		}

		private sealed class SnapshotFixture : IDisposable
		{
			private readonly string mDirectoryPath;

			public string BaseFilePath { get; }
			public string TargetFilePath { get; }

			public SnapshotFixture()
			{
				mDirectoryPath = Path.Combine(
					Path.GetTempPath(),
					"GuiTest",
					"SnapshotComparison",
					Guid.NewGuid().ToString("N"));

				Directory.CreateDirectory(mDirectoryPath);

				BaseFilePath = Path.Combine(mDirectoryPath, "Base.snapshot.json");
				TargetFilePath = Path.Combine(mDirectoryPath, "Target.snapshot.json");

				SaveBase(createSnapshot("Base", new StatusSaveData
				{
					Attack = 1000,
					HP = 10000,
					Defense = 500,
					Speed = 120,
					CritRate = 40,
					CritDamage = 150,
					EffectHit = 20,
					EffectResist = 10
				}));

				SaveTarget(createSnapshot("Target", new StatusSaveData
				{
					Attack = 1300,
					HP = 9000,
					Defense = 550,
					Speed = 135,
					CritRate = 55,
					CritDamage = 140,
					EffectHit = 20,
					EffectResist = 5
				}));
			}

			public void SaveBase(CalculationSnapshotSaveData data)
			{
				SaveDataAccess.SaveSnapshot(BaseFilePath, data);
			}

			public void SaveTarget(CalculationSnapshotSaveData data)
			{
				SaveDataAccess.SaveSnapshot(TargetFilePath, data);
			}

			public void Dispose()
			{
				Directory.Delete(mDirectoryPath, true);
			}
		}
	}
}
