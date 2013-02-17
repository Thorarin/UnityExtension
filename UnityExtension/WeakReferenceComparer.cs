using System;
using System.Collections.Generic;

namespace UnityExtension
{
    public class WeakReferenceComparer : IEqualityComparer<WeakReference>
    {
        public bool Equals(WeakReference x, WeakReference y)
        {
            return (x.IsAlive && y.IsAlive && x.Target == y.Target);
        }

        public int GetHashCode(WeakReference obj)
        {
            if (obj.IsAlive)
            {
                return obj.Target.GetHashCode();
            }

            return base.GetHashCode();
        }
    }
}