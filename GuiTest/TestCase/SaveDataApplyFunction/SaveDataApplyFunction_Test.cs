using Gui.ControlSet;
using Gui.Converter;
using Gui.Definition;
using Gui.Dialog;
using Gui.Form.Control;
using Gui.Function;
using Gui.Logging;
using Gui.Model;
using Gui.Operator;
using GuiTest.TestData;
using GuiTest.TestUtil;
using System.Windows.Forms;

namespace GuiTest.TestCase
{
	[TestClass]
	[DoNotParallelize]
	public class SaveDataApplyFunction_Test
	{
		[TestMethod]
		public void ApplyBuildSuccess()
		{
			StaTestRunner.Run(() =>
			{
				using (var fixture = new ApplyFixture(null))
				{
					MitamaSetInputModel target = loadValidInputModel();

					fixture.ApplyFunction.Apply(SaveDataLoadType.Build, "After", target);

					Assert.AreEqual("After", fixture.ShikigamiOperator.GetSelectedShikigami()?.Name);

					assertInputEqual(target, fixture.MitamaOperator.CreateInputModel());
				}
			});
		}

		[TestMethod]
		public void ApplyFailureRollbackSuccess()
		{
			StaTestRunner.Run(() =>
			{
				ApplyFixture fixture = null;
				MitamaSetInputModel appliedInput = null;
				string appliedShikigamiName = null;
				bool afterApplyReached = false;
				bool rollbackReached = false;

				fixture = new ApplyFixture(checkpoint =>
				{
					if (checkpoint == SaveDataApplyCheckpoint.AfterApply)
					{
						afterApplyReached = true;

						appliedShikigamiName = fixture.ShikigamiOperator.GetSelectedShikigami()?.Name;
						appliedInput = fixture.MitamaOperator.CreateInputModel();

						throw new InvalidOperationException("Injected apply failure.");
					}

					if (checkpoint == SaveDataApplyCheckpoint.BeforeRollbackRestore)
					{
						rollbackReached = true;
					}
				});

				using (fixture)
				{
					// 適用前の御魂データAを実際のコントロールへ設定
					MitamaSetInputModel originalInput = loadValidInputModel();

					fixture.MitamaOperator.ApplyInputModel(originalInput);

					MitamaSetInputModel previousInput = fixture.MitamaOperator.CreateInputModel();

					assertInputEqual(originalInput, previousInput);

					Assert.AreEqual("Before", fixture.ShikigamiOperator.GetSelectedShikigami()?.Name);

					// 御魂データAとは異なるデータBを作成
					MitamaSetInputModel target = loadValidInputModel();

					target.Mitamas[0].MainStat.ValueText = "555";

					target.Mitamas[0].SubStat[0].Type = DisplayText.SPEED;
					target.Mitamas[0].SubStat[0].ValueText = "9";

					target.Mitamas[1].MainStat.Type = DisplayText.ADDITIONAL_ATTACK_RATE;
					target.Mitamas[1].MainStat.ValueText = "55";

					target.SetEffects[0].Stat.Type = DisplayText.EFFECT_RESIST;

					// AとBが異なることを確認
					Assert.AreNotEqual(previousInput.Mitamas[0].MainStat.ValueText, target.Mitamas[0].MainStat.ValueText);

					Assert.AreNotEqual(previousInput.SetEffects[0].Stat.Type, target.SetEffects[0].Stat.Type);

					try
					{
						fixture.ApplyFunction.Apply(SaveDataLoadType.Build, "After", target);

						Assert.Fail("Apply failure was not detected.");
					}
					catch (InvalidOperationException ex)
					{
						Assert.AreEqual("Injected apply failure.", ex.Message);
					}

					Assert.IsTrue(afterApplyReached);
					Assert.IsTrue(rollbackReached);

					// 例外発生時点ではデータBが適用されていたことを確認
					Assert.AreEqual("After", appliedShikigamiName);

					assertInputEqual(target, appliedInput);

					// ロールバック後はデータAに完全復元されていることを確認
					Assert.AreEqual("Before", fixture.ShikigamiOperator.GetSelectedShikigami()?.Name);

					assertInputEqual(previousInput, fixture.MitamaOperator.CreateInputModel());
				}
			});
		}

		[TestMethod]
		public void ApplyFailureRollbackFailure()
		{
			StaTestRunner.Run(() =>
			{
				bool applyFailureReached = false;
				bool rollbackFailureReached = false;

				using (var fixture = new ApplyFixture(checkpoint =>
				{
					if (checkpoint == SaveDataApplyCheckpoint.AfterShikigamiSelection)
					{
						applyFailureReached = true;
						throw new InvalidOperationException("Injected apply failure.");
					}

					if (checkpoint == SaveDataApplyCheckpoint.BeforeRollbackRestore)
					{
						rollbackFailureReached = true;
						throw new InvalidOperationException("Injected rollback failure.");
					}
				}))
				{
					MitamaSetInputModel previousInput = fixture.MitamaOperator.CreateInputModel();
					MitamaSetInputModel target = loadValidInputModel();

					try
					{
						fixture.ApplyFunction.Apply(SaveDataLoadType.Build, "After", target);

						Assert.Fail("Rollback failure was not detected.");
					}
					catch (SaveDataRollbackException ex)
					{
						AggregateException errors = ex.InnerException as AggregateException;

						Assert.IsNotNull(errors);
						Assert.AreEqual(2, errors.InnerExceptions.Count);
						Assert.AreEqual("Injected apply failure.", errors.InnerExceptions[0].Message);
						Assert.AreEqual("Injected rollback failure.", errors.InnerExceptions[1].Message);
					}

					Assert.IsTrue(applyFailureReached);
					Assert.IsTrue(rollbackFailureReached);

					// 式神だけ変更され、復元には失敗した状態を確認
					Assert.AreEqual("After", fixture.ShikigamiOperator.GetSelectedShikigami()?.Name);

					assertInputEqual(previousInput, fixture.MitamaOperator.CreateInputModel());
				}
			});
		}

		private static MitamaSetInputModel loadValidInputModel()
		{
			return MitamaSetConverter.ToInputModel(TestDataLoader.LoadValidMitamaSet());
		}

		private static void assertInputEqual(MitamaSetInputModel expected, MitamaSetInputModel actual)
		{
			Assert.IsNotNull(expected);
			Assert.IsNotNull(actual);

			Assert.AreEqual(expected.Mitamas.Count, actual.Mitamas.Count);

			for (int i = 0; i < expected.Mitamas.Count; i++)
			{
				assertStatEqual(expected.Mitamas[i].MainStat, actual.Mitamas[i].MainStat);

				Assert.AreEqual(expected.Mitamas[i].SubStat.Count, actual.Mitamas[i].SubStat.Count);

				for (int j = 0; j < expected.Mitamas[i].SubStat.Count; j++)
				{
					assertStatEqual(expected.Mitamas[i].SubStat[j], actual.Mitamas[i].SubStat[j]);
				}
			}

			Assert.AreEqual(expected.SetEffects.Count, actual.SetEffects.Count);

			for (int i = 0; i < expected.SetEffects.Count; i++)
			{
				assertStatEqual(expected.SetEffects[i].Stat, actual.SetEffects[i].Stat);
			}

			Assert.AreEqual(expected.UniqueEffects.Count, actual.UniqueEffects.Count);

			for (int i = 0; i < expected.UniqueEffects.Count; i++)
			{
				assertStatEqual(expected.UniqueEffects[i].Stat, actual.UniqueEffects[i].Stat);
			}
		}

		private static void assertStatEqual(StatValueInputModel expected, StatValueInputModel actual)
		{
			Assert.AreEqual(expected.Type, actual.Type);
			Assert.AreEqual(expected.ValueText, actual.ValueText);
		}

		private sealed class ApplyFixture : IDisposable
		{
			private readonly List<Control> mControls = new List<Control>();

			public MitamaInputOperator MitamaOperator { get; }
			public ShikigamiSelectionOperator ShikigamiOperator { get; }
			public SaveDataApplyFunction ApplyFunction { get; }

			public ApplyFixture(Action<SaveDataApplyCheckpoint> checkpoint)
			{
				Directory.CreateDirectory(AppPath.LogDirectoryPath);

				var tracker = new PerformanceTracker();
				var comboBoxOperator = new ComboBoxOperator(tracker);

				var slots = new MitamaSlotInputControl[6];

				for (int i = 0; i < slots.Length; i++)
				{
					var subStats = new SubStatInputControl[4];

					for (int j = 0; j < subStats.Length; j++)
					{
						subStats[j] = new SubStatInputControl
						{
							TypeComboBox = createComboBox(),
							ValueTextBox = createTextBox()
						};
					}

					slots[i] = new MitamaSlotInputControl
					{
						MainStatComboBox = createComboBox(),
						MainValueTextBox = createTextBox(),
						SubStats = subStats
					};
				}

				var setEffects = new ComboBox[3];
				var uniqueEffects = new ComboBox[6];

				for (int i = 0; i < setEffects.Length; i++)
				{
					setEffects[i] = createComboBox();
				}

				for (int i = 0; i < uniqueEffects.Length; i++)
				{
					uniqueEffects[i] = createComboBox();
				}

				var mitamaControls = new MitamaInputControlSet(slots, setEffects, uniqueEffects);

				MitamaOperator = new MitamaInputOperator(mitamaControls, comboBoxOperator, tracker);

				MitamaOperator.InitializeMainStatComboBoxes();
				MitamaOperator.InitializeSubStatComboBoxes();
				MitamaOperator.InitializeSetEffectComboBoxes();
				MitamaOperator.InitializeUniqueEffectComboBoxes();

				var shikigamiComboBox = createComboBox();
				var rarityComboBox = createComboBox();

				rarityComboBox.Items.Add(DisplayText.RARITY_ALL);
				rarityComboBox.SelectedIndex = 0;

				var shikigamiControls = new ShikigamiSelectionControlSet(shikigamiComboBox, rarityComboBox, createTextBox());

				ShikigamiOperator = new ShikigamiSelectionOperator(shikigamiControls, comboBoxOperator, tracker);

				ShikigamiOperator.SetShikigamiDisplayMember("Name");

				ShikigamiOperator.SetShikigamiItems(new List<ShikigamiDto>
				{
					new ShikigamiDto { Name = "Before", Rarity = DisplayText.RARITY_SSR },
					new ShikigamiDto { Name = "After", Rarity = DisplayText.RARITY_SSR }
				});

				ShikigamiOperator.TrySelectShikigami("Before");

				var shikigamiFunction = new ShikigamiSelectionFunction(ShikigamiOperator, tracker, source => { });

				ApplyFunction = new SaveDataApplyFunction(MitamaOperator, ShikigamiOperator, shikigamiFunction, tracker, checkpoint);
			}

			private ComboBox createComboBox()
			{
				var comboBox = new ComboBox();
				mControls.Add(comboBox);
				return comboBox;
			}

			private TextBox createTextBox()
			{
				var textBox = new TextBox();
				mControls.Add(textBox);
				return textBox;
			}

			public void Dispose()
			{
				foreach (Control control in mControls)
				{
					control.Dispose();
				}
			}
		}
	}
}
