namespace CoreTestDataConverter.Model
{
	public abstract class TestData<TInput, TExpected>
	{
		public TInput Input { get; set; }

		public TExpected Expected { get; set; }
	}
}
