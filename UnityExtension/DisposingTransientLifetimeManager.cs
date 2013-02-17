using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityExtension
{
    public class DisposingTransientLifetimeManager : DisposingLifetimeManager, IDisposable
    {
        private readonly List<WeakReference> _values = new List<WeakReference>();

        public override object GetValue()
        {
            return null;
        }

        public override void SetValue(object newValue)
        {
            RemoveDeadReferences();
            _values.Add(new WeakReference(newValue));
        }

        public override void RemoveValue()
        {
        }

        public override bool AppliesTo(object instance)
        {
            return _values.Any(wr => wr.Target == instance);
        }

        public override void RemoveValue(object instance)
        {
            // Silverlight does not have FindIndex(...) method, so we use an old fashioned loop.
            for (int i = _values.Count - 1; i >= 0; i--)
            {
                if (_values[i].Target == instance)
                {
                    _values.RemoveAt(i);
                    break;
                }
            }

            RemoveDeadReferences();
        }

        public void Dispose()
        {
            // Class must be IDisposable to be retained in the list of Lifetime Managers
            _values.Clear();
        }

        private void RemoveDeadReferences()
        {
            for (int i = _values.Count - 1; i >= 0; i--)
            {
                if (!_values[i].IsAlive)
                    _values.RemoveAt(i);
            }
        }
    }
}