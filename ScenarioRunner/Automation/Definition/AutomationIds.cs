namespace ScenarioRunner.Automation.Definition
{
	internal static class AutomationIds
	{
		internal static class MainForm
		{
			public const string ID = "MainForm";

			public const string SHIKIGAMI = "cmbShikigami";

			public static string MainStat(int mitamaSlot)
			{
				return $"cmbMainStat{mitamaSlot}";
			}

			public static string SubStat(int mitamaSlot, int subSlot)
			{
				return $"cmbSubStat{subSlot}{mitamaSlot}";
			}

			public static string SubStatValue(int mitamaSlot, int subSlot)
			{
				return $"txtSubVal{subSlot}{mitamaSlot}";
			}

			public static string SetEffect(int slot)
			{
				return $"cmbSetBonus{slot}";
			}

			public static string UniqueEffect(int slot)
			{
				return $"cmbUnique{slot}";
			}

			public const string CALCULATE = "btnCalc";
			public const string MITAMA_ONLY = "txtMitamaOnly";
			public const string FINAL_STATS = "txtFinalStats";

			public const string CLEAR = "btnClear";

			public const string LOAD = "btnLoad";

			public const string RELOAD_SHIKIGAMI = "btnReLoad";
			public const string EDIT_SHIKIGAMI = "btnEditShikigami";
			public const string SHIKIGAMI_RECOVERY = "btnRecoveryShikigami";
		}

		internal static class SaveDataLoadDialog
		{
			public const string ID = "SaveDataLoadDialog";

			public const string LOAD_TYPE = "cmbLoadType";
			public const string BROWSE = "btnBrowse";
			public const string LOAD = "btnLoad";
		}

		internal static class ShikigamiRegisterForm
		{
			public const string ID = "ShikigamiRegisterForm";

			public const string REGISTER = "btnRegister";
		}

		internal static class ShikigamiRecoveryDialog
		{
			public const string ID = "ShikigamiRecoveryDialog";

			public const string RECOVERY = "btnRecovery";
		}

		internal static class MessageBox
		{
			public const string YES = "6";
		}
	}
}
