using System;
using System.Threading;

namespace UnityExtension.Tests.TestClasses
{
    public class DisposableClass : IDisposable
    {
        private static int _instanceCount;

        private bool _disposed;

        public DisposableClass()
        {
            Interlocked.Increment(ref _instanceCount);
        }

        public static int InstanceCount
        {
            get { return _instanceCount; }
        }

        public bool Disposed
        {
            get { return _disposed; }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("Object is already disposed.");
            }

            _disposed = true;
        }

        ~DisposableClass()
        {
            Interlocked.Decrement(ref _instanceCount);
        }
    }
}