using System;
using System.Collections.Generic;
using System.Text;

namespace GPSoftware.Core {

    /// <summary>
    ///     Class offering a start and end <see cref="DateTimeOffset"/> and its <see cref="TimeSpan"/>
    /// </summary>
    [Serializable]
    public class TimeFrameOffset {

        public DateTimeOffset Start { get; set; }
        public DateTimeOffset End { get; set; }
        public TimeSpan TimeSpan => End - Start;

        public TimeFrameOffset() {
        }

        public TimeFrameOffset(DateTimeOffset start, DateTimeOffset end) {
            Start = start;
            End = end;
        }

        public override int GetHashCode() {
            unchecked {
                int hash = 17;
                hash = hash * 23 + Start.GetHashCode();
                hash = hash * 23 + End.GetHashCode();
                return hash;
            }
        }

        public override bool Equals(object? obj) {
            if (obj == null) return false;
            if (!(obj is TimeFrameOffset tf)) return false;
            return (Start == tf.Start) && (End == tf.End);
        }

        public override string ToString() {
            return $"{Start} - {End}";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format">A standard or custom date and time format string.</param>
        public virtual string ToString(string format) {
            return $"{Start.ToString(format)} - {End.ToString(format)}";
        }
    }
}
