using Gui.Logging;

namespace GuiTest.TestCase
{
	[TestClass]
	public class LogWriter_Test
	{
		[TestMethod]
		public void ConstructorRejectsNullAppend()
		{
			ArgumentNullException exception = Assert.ThrowsExactly<ArgumentNullException>(() => new LogWriter(null));

			Assert.AreEqual("append", exception.ParamName);
		}

		[TestMethod]
		public void TryWriteSuccess()
		{
			string writtenLine = null;
			int writeCount = 0;

			var writer = new LogWriter(line =>
			{
				writtenLine = line;
				writeCount++;
			});

			bool result = writer.TryWrite("Test log");

			Assert.IsTrue(result);
			Assert.AreEqual("Test log", writtenLine);
			Assert.AreEqual(1, writeCount);
		}

		[TestMethod]
		public void TryWriteIOException()
		{
			var writer = new LogWriter(line =>
			{
				throw new IOException("Injected write failure.");
			});

			bool result = writer.TryWrite("Test log");

			Assert.IsFalse(result);
		}

		[TestMethod]
		public void TryWriteUnauthorizedAccess()
		{
			var writer = new LogWriter(line =>
			{
				throw new UnauthorizedAccessException("Injected access failure.");
			});

			bool result = writer.TryWrite("Test log");

			Assert.IsFalse(result);
		}

		[TestMethod]
		public void TryWriteDoesNotRetryAfterFailure()
		{
			int writeCount = 0;

			var writer = new LogWriter(line =>
			{
				writeCount++;

				throw new IOException("Injected write failure.");
			});

			bool result = writer.TryWrite("Test log");

			Assert.IsFalse(result);
			Assert.AreEqual(1, writeCount);
		}

		[TestMethod]
		public void TryWriteSuccessAfterFailure()
		{
			int writeCount = 0;
			string writtenLine = null;

			var writer = new LogWriter(line =>
			{
				writeCount++;

				if (writeCount == 1)
				{
					throw new IOException("Injected first write failure.");
				}

				writtenLine = line;
			});

			bool firstResult = writer.TryWrite("First");
			bool secondResult = writer.TryWrite("Second");

			Assert.IsFalse(firstResult);
			Assert.IsTrue(secondResult);

			Assert.AreEqual(2, writeCount);
			Assert.AreEqual("Second", writtenLine);
		}

		[TestMethod]
		public void TryWritePreservesCallOrder()
		{
			var writtenLines = new List<string>();

			var writer = new LogWriter(line =>
			{
				writtenLines.Add(line);
			});

			Assert.IsTrue(writer.TryWrite("First"));
			Assert.IsTrue(writer.TryWrite("Second"));
			Assert.IsTrue(writer.TryWrite("Third"));

			CollectionAssert.AreEqual(
				new List<string>
				{
					"First",
					"Second",
					"Third"
				},
				writtenLines);
		}

		[TestMethod]
		public void TryWriteSerializesConcurrentCalls()
		{
			const int WRITE_COUNT = 100;

			int activeWrites = 0;
			int overlappingWrites = 0;
			int completedWrites = 0;
			int failedWrites = 0;

			var writer = new LogWriter(line =>
			{
				int active = Interlocked.Increment(ref activeWrites);

				try
				{
					if (active > 1)
					{
						Interlocked.Exchange(ref overlappingWrites, 1);
					}

					Thread.Sleep(1);

					Interlocked.Increment(ref completedWrites);
				}
				finally
				{
					Interlocked.Decrement(ref activeWrites);
				}
			});

			Parallel.For(0, WRITE_COUNT, i =>
			{
				if (!writer.TryWrite($"Log {i}"))
				{
					Interlocked.Increment(ref failedWrites);
				}
			});

			Assert.AreEqual(0, failedWrites);
			Assert.AreEqual(WRITE_COUNT, completedWrites);
			Assert.AreEqual(0, activeWrites);
			Assert.AreEqual(0, overlappingWrites);
		}
	}
}
