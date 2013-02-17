using System;

namespace UnityExtension.Tests.TestClasses
{
    public class ClassWithTwoDisposables
    {
        private readonly DisposableClass _disposable;
        private readonly DisposableClass _disposable2;

        public ClassWithTwoDisposables(DisposableClass disposable, DisposableClass disposable2)
        {
            _disposable2 = disposable2;
            _disposable = disposable;
        }

        public DisposableClass Disposable
        {
            get { return _disposable; }
        }

        public DisposableClass Disposable2
        {
            get { return _disposable2; }
        }
    }
}