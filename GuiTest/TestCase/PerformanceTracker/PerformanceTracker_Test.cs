using Gui.Logging;

namespace GuiTest.TestCase
{
	[TestClass]
	public class PerformanceTracker_Test
	{
		[TestMethod]
		public void TrackRejectsInvalidTarget()
		{
			var tracker = new PerformanceTracker();

			Assert.ThrowsExactly<ArgumentException>(() => tracker.Track(null));
			Assert.ThrowsExactly<ArgumentException>(() => tracker.Track(""));
			Assert.ThrowsExactly<ArgumentException>(() => tracker.Track("   "));

			Assert.AreEqual(0, tracker.GetSnapshot().Count);
		}

		[TestMethod]
		public void GetSnapshotInitiallyEmpty()
		{
			var tracker = new PerformanceTracker();

			Assert.AreEqual(0, tracker.GetSnapshot().Count);
		}

		[TestMethod]
		public void TrackRecordsSingleMeasurement()
		{
			var tracker = new PerformanceTracker();

			using (tracker.Track("Test"))
			{
			}

			var snapshot = tracker.GetSnapshot();

			Assert.AreEqual(1, snapshot.Count);
			Assert.AreEqual("Test", snapshot[0].Target);
			Assert.AreEqual(1L, snapshot[0].Count);

			Assert.IsTrue(snapshot[0].TotalTicks >= 0);
			Assert.AreEqual(snapshot[0].TotalTicks, snapshot[0].MaxTicks);
		}

		[TestMethod]
		public void TrackAggregatesSameTarget()
		{
			var tracker = new PerformanceTracker();

			for (int i = 0; i < 5; i++)
			{
				using (tracker.Track("Test"))
				{
				}
			}

			var snapshot = tracker.GetSnapshot();

			Assert.AreEqual(1, snapshot.Count);
			Assert.AreEqual(5L, snapshot[0].Count);

			Assert.IsTrue(snapshot[0].TotalTicks >= snapshot[0].MaxTicks);
			Assert.IsTrue(snapshot[0].MaxTicks >= 0);
		}

		[TestMethod]
		public void TrackSeparatesDifferentTargets()
		{
			var tracker = new PerformanceTracker();

			using (tracker.Track("TargetA"))
			{
			}

			using (tracker.Track("TargetB"))
			{
			}

			using (tracker.Track("TargetA"))
			{
			}

			var snapshot = tracker.GetSnapshot();

			Assert.AreEqual(2, snapshot.Count);

			Assert.AreEqual("TargetA", snapshot[0].Target);
			Assert.AreEqual(2L, snapshot[0].Count);

			Assert.AreEqual("TargetB", snapshot[1].Target);
			Assert.AreEqual(1L, snapshot[1].Count);
		}

		[TestMethod]
		public void DisposeRecordsMeasurementOnlyOnce()
		{
			var tracker = new PerformanceTracker();

			IDisposable scope = tracker.Track("Test");

			scope.Dispose();

			var firstSnapshot = tracker.GetSnapshot();

			scope.Dispose();

			var secondSnapshot = tracker.GetSnapshot();

			Assert.AreEqual(1L, firstSnapshot[0].Count);
			Assert.AreEqual(1L, secondSnapshot[0].Count);

			Assert.AreEqual(firstSnapshot[0].TotalTicks, secondSnapshot[0].TotalTicks);
			Assert.AreEqual(firstSnapshot[0].MaxTicks, secondSnapshot[0].MaxTicks);
		}

		[TestMethod]
		public void TrackRecordsMeasurementOnException()
		{
			var tracker = new PerformanceTracker();

			Assert.ThrowsExactly<InvalidOperationException>(() =>
			{
				using (tracker.Track("Test"))
				{
					throw new InvalidOperationException("Injected failure.");
				}
			});

			var snapshot = tracker.GetSnapshot();

			Assert.AreEqual(1, snapshot.Count);
			Assert.AreEqual("Test", snapshot[0].Target);
			Assert.AreEqual(1L, snapshot[0].Count);
		}

		[TestMethod]
		public void TrackRecordsNestedMeasurements()
		{
			var tracker = new PerformanceTracker();

			using (tracker.Track("Outer"))
			{
				using (tracker.Track("Inner"))
				{
				}
			}

			var snapshot = tracker.GetSnapshot();

			Assert.AreEqual(2, snapshot.Count);

			Assert.AreEqual("Inner", snapshot[0].Target);
			Assert.AreEqual(1L, snapshot[0].Count);

			Assert.AreEqual("Outer", snapshot[1].Target);
			Assert.AreEqual(1L, snapshot[1].Count);

			Assert.IsTrue(snapshot[1].TotalTicks >= snapshot[0].TotalTicks);
		}

		[TestMethod]
		public void GetSnapshotReturnsIndependentCopy()
		{
			var tracker = new PerformanceTracker();

			using (tracker.Track("Test"))
			{
			}

			var firstSnapshot = tracker.GetSnapshot();

			long firstTotalTicks = firstSnapshot[0].TotalTicks;
			long firstMaxTicks = firstSnapshot[0].MaxTicks;

			using (tracker.Track("Test"))
			{
			}

			var secondSnapshot = tracker.GetSnapshot();

			Assert.AreEqual(1L, firstSnapshot[0].Count);
			Assert.AreEqual(firstTotalTicks, firstSnapshot[0].TotalTicks);
			Assert.AreEqual(firstMaxTicks, firstSnapshot[0].MaxTicks);

			Assert.AreEqual(2L, secondSnapshot[0].Count);

			Assert.AreNotSame(firstSnapshot[0], secondSnapshot[0]);
		}

		[TestMethod]
		public void GetSnapshotDoesNotExposeInternalStatistics()
		{
			var tracker = new PerformanceTracker();

			using (tracker.Track("Test"))
			{
			}

			var snapshot = tracker.GetSnapshot();

			long expectedTotalTicks = snapshot[0].TotalTicks;
			long expectedMaxTicks = snapshot[0].MaxTicks;

			snapshot[0].Target = "Modified";
			snapshot[0].Count = 999;
			snapshot[0].TotalTicks = 999;
			snapshot[0].MaxTicks = 999;

			var actual = tracker.GetSnapshot();

			Assert.AreEqual("Test", actual[0].Target);
			Assert.AreEqual(1L, actual[0].Count);
			Assert.AreEqual(expectedTotalTicks, actual[0].TotalTicks);
			Assert.AreEqual(expectedMaxTicks, actual[0].MaxTicks);
		}

		[TestMethod]
		public void GetSnapshotSortsByTarget()
		{
			var tracker = new PerformanceTracker();

			using (tracker.Track("Z"))
			{
			}

			using (tracker.Track("A"))
			{
			}

			using (tracker.Track("M"))
			{
			}

			var snapshot = tracker.GetSnapshot();

			Assert.AreEqual(3, snapshot.Count);

			Assert.AreEqual("A", snapshot[0].Target);
			Assert.AreEqual("M", snapshot[1].Target);
			Assert.AreEqual("Z", snapshot[2].Target);
		}

		[TestMethod]
		public void TrackAggregatesConcurrentMeasurements()
		{
			const int THREAD_COUNT = 8;
			const int MEASUREMENTS_PER_THREAD = 100;

			var tracker = new PerformanceTracker();

			Parallel.For(0, THREAD_COUNT, i =>
			{
				for (int j = 0; j < MEASUREMENTS_PER_THREAD; j++)
				{
					using (tracker.Track("Concurrent"))
					{
					}
				}
			});

			var snapshot = tracker.GetSnapshot();

			Assert.AreEqual(1, snapshot.Count);
			Assert.AreEqual("Concurrent", snapshot[0].Target);
			Assert.AreEqual((long)THREAD_COUNT * MEASUREMENTS_PER_THREAD, snapshot[0].Count);

			Assert.IsTrue(snapshot[0].TotalTicks >= snapshot[0].MaxTicks);
			Assert.IsTrue(snapshot[0].MaxTicks >= 0);
		}

		[TestMethod]
		public void GetSnapshotDuringConcurrentMeasurements()
		{
			const int MEASUREMENT_COUNT = 1000;

			var tracker = new PerformanceTracker();

			Task measurementTask = Task.Run(() =>
			{
				for (int i = 0; i < MEASUREMENT_COUNT; i++)
				{
					using (tracker.Track("Concurrent"))
					{
					}
				}
			});

			while (!measurementTask.IsCompleted)
			{
				var snapshot = tracker.GetSnapshot();

				Assert.IsTrue(snapshot.Count <= 1);

				if (snapshot.Count == 1)
				{
					Assert.IsTrue(snapshot[0].Count >= 1);
					Assert.IsTrue(snapshot[0].Count <= MEASUREMENT_COUNT);
					Assert.IsTrue(snapshot[0].TotalTicks >= snapshot[0].MaxTicks);
				}
			}

			measurementTask.GetAwaiter().GetResult();

			var finalSnapshot = tracker.GetSnapshot();

			Assert.AreEqual(1, finalSnapshot.Count);
			Assert.AreEqual((long)MEASUREMENT_COUNT, finalSnapshot[0].Count);
		}
	}
}
