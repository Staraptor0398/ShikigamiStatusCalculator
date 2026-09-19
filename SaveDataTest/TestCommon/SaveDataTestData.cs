using SaveData.Model;

namespace SaveDataTest.TestCommon
{
	public static class SaveDataTestData
	{
		public const string V1_MITAMA_SET_JSON = @"{
  ""Mitamas"": [
    {
      ""Slot"": 1,
      ""MainStat"": {
        ""Type"": ""攻撃力"",
        ""Value"": 486.0
      },
      ""SubStats"": [
        {
          ""Type"": ""会心率"",
          ""Value"": 10.0
        }
      ]
    }
  ],
  ""SetEffects"": [
    {
      ""Type"": ""会心率"",
      ""Value"": 15.0
    }
  ],
  ""UniqueEffects"": [
    {
      ""Type"": ""効果命中"",
      ""Value"": 15.0
    }
  ]
}";

		public const string V1_BUILD_JSON = @"{
  ""ShikigamiName"": ""テスト式神"",
  ""MitamaSet"": {
    ""Mitamas"": [
      {
        ""Slot"": 1,
        ""MainStat"": {
          ""Type"": ""攻撃力"",
          ""Value"": 486.0
        },
        ""SubStats"": [
          {
            ""Type"": ""会心率"",
            ""Value"": 10.0
          }
        ]
      }
    ],
    ""SetEffects"": [
      {
        ""Type"": ""会心率"",
        ""Value"": 15.0
      }
    ],
    ""UniqueEffects"": [
      {
        ""Type"": ""効果命中"",
        ""Value"": 15.0
      }
    ]
  }
}";

		public const string V1_SNAPSHOT_JSON = @"{
  ""SnapshotName"": ""TestSnapshot"",
  ""CreatedAt"": ""2026-09-19 12:00:00"",
  ""ShikigamiName"": ""テスト式神"",
  ""MitamaSet"": {
    ""Mitamas"": [
      {
        ""Slot"": 1,
        ""MainStat"": {
          ""Type"": ""攻撃力"",
          ""Value"": 486.0
        },
        ""SubStats"": [
          {
            ""Type"": ""会心率"",
            ""Value"": 10.0
          }
        ]
      }
    ],
    ""SetEffects"": [
      {
        ""Type"": ""会心率"",
        ""Value"": 15.0
      }
    ],
    ""UniqueEffects"": [
      {
        ""Type"": ""効果命中"",
        ""Value"": 15.0
      }
    ]
  },
  ""MitamaStatus"": {
    ""Attack"": 1000.0,
    ""HP"": 2000.0,
    ""Deffense"": 300.0,
    ""Speed"": 120.0,
    ""CritRate"": 20.0,
    ""CritDamage"": 170.0,
    ""EffectHit"": 15.0,
    ""EffectResist"": 10.0
  },
  ""FinalStatus"": {
    ""Attack"": 4000.0,
    ""HP"": 12000.0,
    ""Deffense"": 700.0,
    ""Speed"": 130.0,
    ""CritRate"": 30.0,
    ""CritDamage"": 180.0,
    ""EffectHit"": 25.0,
    ""EffectResist"": 20.0
  }
}";

		public const string FUTURE_VERSION_MITAMA_SET_JSON = @"{
  ""Version"": 3,
  ""Data"": {
    ""Mitamas"": [],
    ""SetEffects"": [],
    ""UniqueEffects"": []
  }
}";

		public static MitamaSetSaveData CreateMitamaSet()
		{
			return new MitamaSetSaveData
			{
				Mitamas = new List<MitamaSaveData>
				{
					new MitamaSaveData
					{
						Slot = 1,
						MainStat = new StatValueSaveData { Type = "攻撃力", Value = 486.0 },
						SubStats = new List<StatValueSaveData>
						{
							new StatValueSaveData { Type = "会心率", Value = 10.0 }
						}
					}
				},
				SetEffects = new List<SetEffectSaveData>
				{
					new SetEffectSaveData { Stat = new StatValueSaveData { Type = "会心率", Value = 15.0 } }
				},
				UniqueEffects = new List<SetEffectSaveData>
				{
					new SetEffectSaveData { Stat = new StatValueSaveData { Type = "効果命中", Value = 15.0 } }
				}
			};
		}

		public static BuildSaveData CreateBuild()
		{
			return new BuildSaveData
			{
				ShikigamiName = "テスト式神",
				MitamaSet = CreateMitamaSet()
			};
		}

		public static CalculationSnapshotSaveData CreateSnapshot()
		{
			return new CalculationSnapshotSaveData
			{
				SnapshotName = "TestSnapshot",
				CreatedAt = "2026-09-19 12:00:00",
				ShikigamiName = "テスト式神",
				MitamaSet = CreateMitamaSet(),
				MitamaStatus = new StatusSaveData
				{
					Attack = 1000.0,
					HP = 2000.0,
					Defense = 300.0,
					Speed = 120.0,
					CritRate = 20.0,
					CritDamage = 170.0,
					EffectHit = 15.0,
					EffectResist = 10.0
				},
				FinalStatus = new StatusSaveData
				{
					Attack = 4000.0,
					HP = 12000.0,
					Defense = 700.0,
					Speed = 130.0,
					CritRate = 30.0,
					CritDamage = 180.0,
					EffectHit = 25.0,
					EffectResist = 20.0
				}
			};
		}
	}
}
