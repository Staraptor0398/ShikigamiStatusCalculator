namespace GuiTest.TestCommon
{
	public static class TestDirectory
	{
		public static string Root => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData");

		public static string MitamaSet => Path.Combine(Root, "MitamaSet");
	}
}
