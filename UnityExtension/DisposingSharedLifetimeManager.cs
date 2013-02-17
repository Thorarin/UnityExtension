using System;

namespace UnityExtension
{
    public class DisposingSharedLifetimeManager : DisposingLifetimeManager, IDisposable
    {
        private object _object;

        public void Dispose()
        {
        }

        public override object GetValue()
        {
            return _object;
        }

        public override void RemoveValue()
        {
            _object = null;
        }

        public override void SetValue(object newValue)
        {
            _object = newValue;
        }

        public override bool AppliesTo(object instance)
        {
            return instance == _object;
        }

        public override void RemoveValue(object instance)
        {
            RemoveValue();
        }
    }
}