using System;
using System.Collections.Generic;

namespace UnityExtension
{
    internal class ReferenceCounter
    {
        private readonly Dictionary<WeakReference, RefCount> _counts =
            new Dictionary<WeakReference, RefCount>(new WeakReferenceComparer());

        private readonly object _syncRoot = new object();

        public int Increment(object obj)
        {
            var wr = new WeakReference(obj);

            lock (_syncRoot)
            {
                RefCount refCount;
                if (!_counts.TryGetValue(wr, out refCount))
                {
                    refCount = new RefCount();
                    _counts[wr] = refCount;
                }

                refCount.Count++;
                return refCount.Count;
            }
        }

        public bool TryDecrement(object obj, out int count)
        {
            var wr = new WeakReference(obj);

            lock (_syncRoot)
            {
                RefCount refCount;
                if (!_counts.TryGetValue(wr, out refCount))
                {
                    count = 0;
                    return false;
                }

                refCount.Count--;
                count = refCount.Count;

                if (count == 0)
                {
                    _counts.Remove(wr);
                }

                return true;
            }
        }

        private class RefCount
        {
            public int Count;

            public override string ToString()
            {
                return Count.ToString();
            }
        }
    }
}