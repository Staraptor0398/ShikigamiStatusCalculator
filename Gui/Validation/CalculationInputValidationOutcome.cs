namespace Gui.Validation
{
	public enum CalculationInputValidationOutcome
	{
		SUCCESS,

		// 御魂のみのステータス計算を可能にするため、
		// 式神未選択はエラーとして扱わない。
		// メインステータス未選択は「未装備スロット」として扱うため、
		// メインステータス未選択自体はエラーにしない。
		// ただし、最低でも1つは御魂を装備していることを前提とする。
		NO_EQUIPPED_MITAMA,
		MAIN_STAT_NOT_SELECTED_WITH_SUB_STAT,

		SUB_STAT_TYPE_WHITHOUT_VALUE,
		SUB_STAT_VALUE_WHITHOUT_TYPE,

		INVALID_VALUE,
		NEGATIVE_VALUE,

		DUPLICATE_SUB_STAT,

		EFFECT_SLOT_COUNT_EXCEEDS_EQUIPPED_SLOTS
	}
}
