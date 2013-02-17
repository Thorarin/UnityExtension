using System;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnityExtension.Tests.TestClasses;

namespace UnityExtension.Tests
{
    [TestClass]
    public class CombinedTests
    {
        [TestMethod]
        public void Combined_Test()
        {
            var container = new UnityContainer();
            container.AddNewExtension<DisposableStrategyExtension>();

            container.RegisterType<DisposableClass>("transient", new DisposingTransientLifetimeManager());
            container.RegisterType<DisposableClass>("shared", new DisposingSharedLifetimeManager());

            var transient1 = container.Resolve<DisposableClass>("transient");
            var transient2 = container.Resolve<DisposableClass>("transient");
            Assert.AreNotEqual(transient1, transient2);

            var shared1 = container.Resolve<DisposableClass>("shared");
            Assert.AreNotEqual(transient1, shared1);
            Assert.AreNotEqual(transient2, shared1);

            var shared2 = container.Resolve<DisposableClass>("shared");
            Assert.AreEqual(shared1, shared2);

            container.Teardown(transient1);
            Assert.IsTrue(transient1.Disposed);

            container.Teardown(shared2);
            Assert.IsFalse(shared2.Disposed);

            container.Teardown(shared1);
            Assert.IsTrue(shared1.Disposed);
        }

        [TestMethod]
        public void TransientParentWithSharedChild_Test()
        {
            var container = new UnityContainer();
            container.AddNewExtension<DisposableStrategyExtension>();

            container.RegisterType<ClassWithDisposable>(new DisposingTransientLifetimeManager());
            container.RegisterType<DisposableClass>(new DisposingSharedLifetimeManager());

            var obj1 = container.Resolve<ClassWithDisposable>();
            var obj2 = container.Resolve<ClassWithDisposable>();
            
            container.Teardown(obj1);
            Assert.IsFalse(obj2.Disposable.Disposed);

            container.Teardown(obj2);
            Assert.IsTrue(obj2.Disposable.Disposed);
        }
    }
}
