namespace Gui.Validation
{
	public enum CalculationInputValidationOutcome
	{
		SUCCESS,

		SHIKIGAMI_NOT_SELECTED,

		// メインステータス未選択は「未装備スロット」として扱うため、
		// メインステータス未選択自体はエラーにしない。
		MAIN_STAT_NOT_SELECTED_WITH_SUB_STAT,

		SUB_STAT_TYPE_WHITHOUT_VALUE,
		SUB_STAT_VALUE_WHITHOUT_TYPE,

		INVALID_VALUE,
		NEGATIVE_VALUE,

		DUPLICATE_SUB_STAT,

		EFFECT_SLOT_COUNT_EXCEEDS_EQUIPPED_SLOTS
	}
}
