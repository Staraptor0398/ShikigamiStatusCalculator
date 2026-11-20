using Gui.Function;
using GuiTest.TestUtil;
using System.Text;

namespace GuiTest.TestCase
{
	[TestClass]
	[DoNotParallelize]
	public class ShikigamiRecoveryFunction_Test
	{
		private const string CSV_HEADER = "レア度,式神名,攻撃力,HP,防御力,素早さ,会心率,会心DMG,効果命中,効果抵抗";

		private const string EXISTING = "SP,Existing,100,1000,200,100,10,150,20,30";
		private const string SELECTED = "SSR,Selected,321,1200,210,110,15,160,25,35";
		private const string UNSELECTED = "SR,Unselected,400,1400,220,120,20,170,30,40";

		[TestMethod]
		public void GetRecoveryCandidatesSuccess()
		{
			using (var fixture = new RecoveryFixture())
			{
				var function = fixture.CreateFunction();

				List<ShikigamiDto> candidates;

				ShikigamiDataOutcomeDto outcome = function.GetRecoveryCandidates(fixture.SourceFilePath, out candidates);

				Assert.AreEqual(ShikigamiDataOutcomeDto.SUCCESS, outcome);
				Assert.IsNotNull(candidates);
				Assert.AreEqual(2, candidates.Count);

				Assert.AreEqual("Selected", candidates[0].Name);
				Assert.AreEqual("SSR", candidates[0].Rarity);
				Assert.AreEqual(321.0, candidates[0].Status.Attack);

				Assert.AreEqual("Unselected", candidates[1].Name);
				Assert.AreEqual("SR", candidates[1].Rarity);
				Assert.AreEqual(400.0, candidates[1].Status.Attack);

				Assert.IsFalse(File.Exists(fixture.BackupFilePath));
			}
		}

		[TestMethod]
		public void RecoverySuccess()
		{
			using (var fixture = new RecoveryFixture())
			{
				var function = fixture.CreateFunction();

				string previousCsv = File.ReadAllText(fixture.CurrentFilePath);

				List<ShikigamiDto> candidates;

				ShikigamiDataOutcomeDto candidateOutcome = function.GetRecoveryCandidates(fixture.SourceFilePath, out candidates);

				Assert.AreEqual(ShikigamiDataOutcomeDto.SUCCESS, candidateOutcome);
				Assert.AreEqual(2, candidates.Count);

				ShikigamiDataOutcomeDto recoveryOutcome = function.Recover(new List<ShikigamiDto> { candidates[0] });

				Assert.AreEqual(ShikigamiDataOutcomeDto.SUCCESS, recoveryOutcome);

				// バックアップには復旧前のCSVが保存される
				Assert.IsTrue(File.Exists(fixture.BackupFilePath));
				Assert.AreEqual(previousCsv, File.ReadAllText(fixture.BackupFilePath));

				// 実際のGateway経由で復旧後CSVを読み直す
				List<ShikigamiDto> actual;

				ShikigamiDataOutcomeDto loadOutcome = ShikigamiGateway.GetShikigamiList(fixture.CurrentFilePath, out actual);

				Assert.AreEqual(ShikigamiDataOutcomeDto.SUCCESS, loadOutcome);
				Assert.AreEqual(2, actual.Count);

				// 既存式神は保持されている
				Assert.AreEqual("Existing", actual[0].Name);
				Assert.AreEqual("SP", actual[0].Rarity);
				Assert.AreEqual(100.0, actual[0].Status.Attack);

				// 選択した式神だけが復旧される
				Assert.AreEqual("Selected", actual[1].Name);
				Assert.AreEqual("SSR", actual[1].Rarity);
				Assert.AreEqual(321.0, actual[1].Status.Attack);
			}
		}

		[TestMethod]
		public void GetRecoveryCandidatesFileNotFound()
		{
			using (var fixture = new RecoveryFixture())
			{
				File.Delete(fixture.SourceFilePath);

				var function = fixture.CreateFunction();

				string previousCsv = File.ReadAllText(fixture.CurrentFilePath);

				List<ShikigamiDto> candidates;

				ShikigamiDataOutcomeDto outcome = function.GetRecoveryCandidates(fixture.SourceFilePath, out candidates);

				Assert.AreEqual(ShikigamiDataOutcomeDto.FILE_OPEN_FAILED, outcome);
				Assert.IsNotNull(candidates);
				Assert.AreEqual(0, candidates.Count);

				Assert.AreEqual(previousCsv, File.ReadAllText(fixture.CurrentFilePath));
				Assert.IsFalse(File.Exists(fixture.BackupFilePath));
			}
		}

		[TestMethod]
		public void RecoveryBackupFailure()
		{
			using (var fixture = new RecoveryFixture())
			{
				bool backupCalled = false;

				var function = fixture.CreateFunction(() =>
				{
					backupCalled = true;
					throw new IOException("Injected backup failure.");
				});

				string previousCsv = File.ReadAllText(fixture.CurrentFilePath);

				List<ShikigamiDto> candidates;

				ShikigamiDataOutcomeDto candidateOutcome = function.GetRecoveryCandidates(fixture.SourceFilePath, out candidates);

				Assert.AreEqual(ShikigamiDataOutcomeDto.SUCCESS, candidateOutcome);
				Assert.AreEqual(2, candidates.Count);

				bool exceptionThrown = false;

				try
				{
					function.Recover(new List<ShikigamiDto> { candidates[0] });
				}
				catch (IOException ex)
				{
					Assert.AreEqual("Injected backup failure.", ex.Message);
					exceptionThrown = true;
				}

				Assert.IsTrue(backupCalled);
				Assert.IsTrue(exceptionThrown);

				// バックアップ失敗後に復旧処理が実行されていない
				Assert.AreEqual(previousCsv, File.ReadAllText(fixture.CurrentFilePath));
				Assert.IsFalse(File.Exists(fixture.BackupFilePath));

				List<ShikigamiDto> actual;

				ShikigamiDataOutcomeDto loadOutcome = ShikigamiGateway.GetShikigamiList(fixture.CurrentFilePath, out actual);

				Assert.AreEqual(ShikigamiDataOutcomeDto.SUCCESS, loadOutcome);
				Assert.AreEqual(1, actual.Count);
				Assert.AreEqual("Existing", actual[0].Name);
			}
		}

		[TestMethod]
		public void RecoveryCurrentFileNotFound()
		{
			using (var fixture = new RecoveryFixture())
			{
				var function = fixture.CreateFunction();

				List<ShikigamiDto> candidates;

				ShikigamiDataOutcomeDto outcome = function.GetRecoveryCandidates(fixture.SourceFilePath, out candidates);

				Assert.AreEqual(ShikigamiDataOutcomeDto.SUCCESS, outcome);

				File.Delete(fixture.CurrentFilePath);

				FileNotFoundException ex = Assert.ThrowsExactly<FileNotFoundException>(() =>
				{
					function.Recover(new List<ShikigamiDto> { candidates[0] });
				});

				Assert.AreEqual(fixture.CurrentFilePath, ex.FileName);
				Assert.IsFalse(File.Exists(fixture.BackupFilePath));
			}
		}

		private static string createCsv(params string[] records)
		{
			return CSV_HEADER + "\n" + string.Join("\n", records) + "\n";
		}

		private sealed class RecoveryFixture : IDisposable
		{
			private readonly TemporaryDirectory mDirectory = new TemporaryDirectory("ShikigamiRecovery");

			public string CurrentFilePath { get; }
			public string SourceFilePath { get; }
			public string BackupFilePath { get; }

			public RecoveryFixture()
			{
				CurrentFilePath = Path.Combine(mDirectory.DirectoryPath, "Current.csv");
				SourceFilePath = Path.Combine(mDirectory.DirectoryPath, "Source.csv");
				BackupFilePath = Path.Combine(mDirectory.DirectoryPath, "Backup.csv");

				File.WriteAllText(CurrentFilePath, createCsv(EXISTING), new UTF8Encoding(false));

				File.WriteAllText(SourceFilePath, createCsv(EXISTING, SELECTED, UNSELECTED), new UTF8Encoding(false));
			}

			public ShikigamiRecoveryFunction CreateFunction(Action createBackup = null)
			{
				return new ShikigamiRecoveryFunction(CurrentFilePath, createBackup ?? (() => File.Copy(CurrentFilePath, BackupFilePath, true)));
			}

			public void Dispose()
			{
				mDirectory.Dispose();
			}
		}
	}
}
