using System;
using Microsoft.Practices.Unity;

namespace UnityExtension
{
    public abstract class DisposingLifetimeManager : LifetimeManager
    {
        public abstract bool AppliesTo(object instance);
        public abstract void RemoveValue(object instance);
    }
}