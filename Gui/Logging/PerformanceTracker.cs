using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Gui.Logging
{
	public sealed class PerformanceTracker
	{
		private readonly Dictionary<string, PerformanceStatistics> mStatistics =
			new Dictionary<string, PerformanceStatistics>();

		private readonly object mSyncRoot = new object();

		public IDisposable Track(string target)
		{
			if (string.IsNullOrWhiteSpace(target))
			{
				throw new ArgumentException("Target is empty.", nameof(target));
			}

			return new MeasurementScope(this, target);
		}

		public IReadOnlyList<PerformanceStatistics> GetSnapshot()
		{
			lock (mSyncRoot)
			{
				return mStatistics.Values
					.OrderBy(x => x.Target)
					.Select(x => new PerformanceStatistics
					{
						Target = x.Target,
						Count = x.Count,
						TotalTicks = x.TotalTicks,
						MaxTicks = x.MaxTicks
					})
					.ToArray();
			}
		}

		private void record(string target, long elapsedTicks)
		{
			lock (mSyncRoot)
			{
				if (!mStatistics.TryGetValue(target, out PerformanceStatistics statistics))
				{
					statistics = new PerformanceStatistics
					{
						Target = target
					};

					mStatistics.Add(target, statistics);
				}

				statistics.Count++;
				statistics.TotalTicks += elapsedTicks;

				if (elapsedTicks > statistics.MaxTicks)
				{
					statistics.MaxTicks = elapsedTicks;
				}
			}
		}

		private sealed class MeasurementScope : IDisposable
		{
			private readonly PerformanceTracker mTracker;
			private readonly string mTarget;
			private readonly long mStartTimestamp;

			private bool mDisposed;

			public MeasurementScope(PerformanceTracker tracker, string target)
			{
				mTracker = tracker;
				mTarget = target;
				mStartTimestamp = Stopwatch.GetTimestamp();
			}

			public void Dispose()
			{
				if (mDisposed)
				{
					return;
				}

				long elapsedTicks = Stopwatch.GetTimestamp() - mStartTimestamp;

				mTracker.record(mTarget, elapsedTicks);

				mDisposed = true;
			}
		}
	}
}
