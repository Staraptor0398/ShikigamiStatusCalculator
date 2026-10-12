namespace GuiTest.TestCommon
{
	public static class TestDirectory
	{
		public static string Root => Path.Combine(Path.GetDirectoryName(typeof(TestDirectory).Assembly.Location), "TestData");

		public static string MitamaSet => Path.Combine(Root, "MitamaSet");
	}
}
