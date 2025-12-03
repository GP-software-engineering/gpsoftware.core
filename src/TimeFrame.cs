using System;
using System.Collections.Generic;
using System.Text;

namespace GPSoftware.Core {

    /// <summary>
    ///     Class offering a start and end <see cref="DateTimeOffset"/> and its <see cref="TimeSpan"/>
    /// </summary>
    [Serializable]
    public class TimeFrame : TimeFrameOffset {

        public TimeFrame() {
        }

        public TimeFrame(DateTime start, DateTime end)
            : base(start, end) {
        }

        public override int GetHashCode() {
            unchecked {
                int hash = 29;
                hash = hash * 37 + base.GetHashCode();
                return hash;
            }
        }

        public override bool Equals(object obj) {
            if (obj == null) return false;
            if (!(obj is TimeFrame tf)) return false;
            return (Start == tf.Start) && (End == tf.End);
        }

    }
}
