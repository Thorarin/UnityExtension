using System;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnityExtension.Tests.TestClasses;

namespace UnityExtension.Tests
{
    [TestClass]
    public class SharedResourceLifetimeManagerTests
    {
        //public TestContext TestContext { get; set; }

        [TestMethod]
        public void RegisteredClass_Test()
        {
            var container = new UnityContainer();
            container.AddNewExtension<DisposableStrategyExtension>();

            container.RegisterType<DisposableClass>(new DisposingSharedLifetimeManager());

            var obj1 = container.Resolve<DisposableClass>();
            var obj2 = container.Resolve<DisposableClass>();
            Assert.AreEqual(obj1, obj2);

            container.Teardown(obj1);
            Assert.IsFalse(obj1.Disposed);

            container.Teardown(obj2);
            Assert.IsTrue(obj2.Disposed);
        }

        [TestMethod]
        public void UnregisteredParent_Test()
        {
            var container = new UnityContainer();
            container.AddNewExtension<DisposableStrategyExtension>();

            container.RegisterType<DisposableClass>(new DisposingSharedLifetimeManager());

            var obj1 = container.Resolve<ClassWithDisposable>();
            var obj2 = container.Resolve<ClassWithDisposable>();
            Assert.AreNotEqual(obj1, obj2);

            container.Teardown(obj1);
            Assert.IsFalse(obj1.Disposable.Disposed);

            container.Teardown(obj2);
            Assert.IsTrue(obj2.Disposable.Disposed);
        }

        [TestMethod]
        public void ClassWithTwoDisposables_Test()
        {
            var container = new UnityContainer();
            container.AddNewExtension<DisposableStrategyExtension>();

            container.RegisterType<DisposableClass>(new DisposingSharedLifetimeManager());

            var obj1 = container.Resolve<ClassWithTwoDisposables>();
            Assert.AreEqual(obj1.Disposable, obj1.Disposable2);

            var obj2 = container.Resolve<ClassWithTwoDisposables>();
            Assert.AreEqual(obj1.Disposable, obj2.Disposable);
            Assert.AreEqual(obj1.Disposable, obj2.Disposable2);
            Assert.AreNotEqual(obj1, obj2);

            container.Teardown(obj1);
            Assert.IsFalse(obj1.Disposable.Disposed);

            container.Teardown(obj2);
            Assert.IsTrue(obj2.Disposable.Disposed);
        }

        [TestMethod]
        public void CreateNewAfterDispose_Test()
        {
            var container = new UnityContainer();
            container.AddNewExtension<DisposableStrategyExtension>();

            container.RegisterType<DisposableClass>(new DisposingSharedLifetimeManager());

            var obj1 = container.Resolve<DisposableClass>();
            container.Teardown(obj1);
            Assert.IsTrue(obj1.Disposed, "After teardown of only instance, object should be disposed.");

            var obj2 = container.Resolve<DisposableClass>();
            Assert.AreNotEqual(obj1, obj2, "Object was previously disposed, new instance should be created.");

            var obj3 = container.Resolve<DisposableClass>();
            Assert.AreEqual(obj2, obj3, "Second reference to the same object expected.");
        }

        /// <summary>
        ///     Tests whether objects are disposed when their container is.
        /// </summary>
        [TestMethod]
        public void ContainerDisposed_Test()
        {
            DisposableClass obj1, obj2;

            using (var container = new UnityContainer())
            {
                container.AddNewExtension<DisposableStrategyExtension>();
                container.RegisterType<DisposableClass>(new DisposingSharedLifetimeManager());

                obj1 = container.Resolve<DisposableClass>();
                obj2 = container.Resolve<DisposableClass>();
            }

            Assert.IsTrue(obj1.Disposed);
            Assert.IsTrue(obj2.Disposed);
        }

        [TestMethod]
        public void ChildContainer_Test()
        {
            DisposableClass obj1, obj2;

            using (var container = new UnityContainer())
            {
                container.AddNewExtension<DisposableStrategyExtension>();
                container.RegisterType<DisposableClass>(new DisposingSharedLifetimeManager());

                obj1 = container.Resolve<DisposableClass>();

                using (IUnityContainer childContainer = container.CreateChildContainer())
                {
                    obj2 = childContainer.Resolve<DisposableClass>();
                    Assert.AreEqual(obj1, obj2);
                }
                Assert.IsFalse(obj2.Disposed);
            }

            Assert.IsTrue(obj1.Disposed);
        }

        /// <summary>
        /// Tests functionality when Teardown of a parent object is forgotten and garbage collected.
        /// </summary>
        [TestMethod]
        public void ForgottenTeardown_Test()
        {
            var container = new UnityContainer();
            container.AddNewExtension<DisposableStrategyExtension>();

            container.RegisterType<DisposableClass>(new DisposingSharedLifetimeManager());

            var obj1 = container.Resolve<ClassWithDisposable>();
            var obj2 = container.Resolve<ClassWithDisposable>();
            var obj3 = container.Resolve<ClassWithDisposable>();

            var wr = new WeakReference(obj1);
            obj1 = null;
            
            GC.Collect();
            Assert.IsFalse(wr.IsAlive);

            container.Teardown(obj2);
            Assert.IsFalse(obj2.Disposable.Disposed);

            container.Teardown(obj3);
            Assert.IsTrue(obj2.Disposable.Disposed);
        }
    }
}