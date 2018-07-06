using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TombLib.Utils
{
    /// <summary>Save data type to represent a unique version of an IN MEMORY version of some data.
    /// Can be used for example to give render buffers a time stamp.</summary>
    public struct DataVersion : IComparable, IComparable<DataVersion>, IEquatable<DataVersion>
    {
        public static DataVersion Zero = new DataVersion();
        // It's ulong so it will never overflow by incrementing.
        // (At least not for a couple ten thousand years of spending all CPU time on incrementing)
        // Private to avoid people doing arithmetic, saving it to a file or doing other bad things.
        private ulong _time;

        private static long CurrentTimeStamp = 1; // Start with 1 so that empty instances always compare less.

        // This is thread safe
        public static DataVersion GetNext()
        {
            long time = Interlocked.Increment(ref CurrentTimeStamp);
            return new DataVersion { _time = unchecked((ulong)time) };
        }
        // This is thread safe
        public static  DataVersion GetCurrent()
        {
            long time = Interlocked.Read(ref CurrentTimeStamp);
            return new DataVersion { _time = unchecked((ulong)time) };
        }

        int IComparable.CompareTo(object obj) => CompareTo((DataVersion)obj);
        public int CompareTo(DataVersion other) => _time.CompareTo(other._time);
        public bool Equals(DataVersion other) => other._time == _time;
        public override bool Equals(object other) => other is DataVersion && (((DataVersion)other)._time == _time);
        public override int GetHashCode() => unchecked((int)_time); // The lowest bits are the most often changing, so we can just discard the upper bits for the hash.
        public override string ToString() => "Time stamp " + _time;
        public static bool operator ==(DataVersion first, DataVersion second) => first._time == second._time;
        public static bool operator !=(DataVersion first, DataVersion second) => first._time != second._time;
        public static bool operator <(DataVersion first, DataVersion second) => first._time < second._time;
        public static bool operator >(DataVersion first, DataVersion second) => first._time > second._time;
        public static bool operator <=(DataVersion first, DataVersion second) => first._time <= second._time;
        public static bool operator >=(DataVersion first, DataVersion second) => first._time >= second._time;
    }
}
