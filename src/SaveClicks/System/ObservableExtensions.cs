#region Usings

using System;
using System.Collections.Generic;
using System.Reactive.Linq;

#endregion

namespace SaveClicks.System
{
    public static class ObservableExtensions
    {
        /// <summary>
        ///     Group observable sequence into buffers separated by periods of calm
        /// </summary>
        /// <param name="source">Observable to buffer</param>
        /// <param name="calmDuration">Duration of calm after which to close buffer</param>
        /// <param name="maxCount">Max size to buffer before returning</param>
        /// <param name="maxDuration">Max duration to buffer before returning</param>
        /// <param name="onFirst">Action that is executed when first item is buffered.</param>
        public static IObservable<IList<T>> BufferUntilCalm<T>(this IObservable<T> source, TimeSpan calmDuration, int? maxCount = null,
            TimeSpan? maxDuration = null, Action onFirst = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var closes = source.Throttle(calmDuration);
            if (maxCount != null)
            {
                var overflows = source.Where((x, index) => index + 1 >= maxCount);
                closes = closes.Amb(overflows);
            }
            if (maxDuration != null)
            {
                var ages = source.Delay(maxDuration.Value);
                closes = closes.Amb(ages);
            }

            var windows = source.Window(() => closes);

            if (onFirst != null)
                windows.Subscribe(wnd => onFirst());


            return windows.SelectMany(window => window.ToList());
        }
    }
}