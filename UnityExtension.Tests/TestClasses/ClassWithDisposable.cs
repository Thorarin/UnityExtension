using System;

namespace UnityExtension.Tests.TestClasses
{
    public class ClassWithDisposable
    {
        private readonly DisposableClass _disposable;

        public ClassWithDisposable(DisposableClass disposable)
        {
            _disposable = disposable;
        }

        public DisposableClass Disposable
        {
            get { return _disposable; }
        }
    }
}