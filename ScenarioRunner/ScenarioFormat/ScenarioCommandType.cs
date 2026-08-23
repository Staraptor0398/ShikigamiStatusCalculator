namespace ScenarioRunner.ScenarioFormat
{
	public enum ScenarioCommandType
	{
		OPEN_GUI,
		CLOSE_GUI,
		CLOSE_DIALOG,

		SELECT_SHIKIGAMI,
		LOAD_MITAMA,

		CALCULATE,
		CLEAR,

		RELOAD_SHIKIGAMI,
		BREAK_SHIKIGAMI_HEADER,

		CHECK_CALCULATION,
		CHECK_SHIKIGAMI,
		CHECK_DIALOG
	}
}
