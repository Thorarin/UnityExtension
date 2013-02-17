using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnityExtension.Tests.TestClasses;

namespace UnityExtension.Tests
{
    [TestClass]
    public class DisposingTransientLifetimeManagerTests
    {
        public TestContext Context { get; set; }

        #if SILVERLIGHT
        [TestInitialize]
        public void TestInitialize()
        {
            Context = new Helpers.UnitTestContext();
        }
        #else
        public TestContext TestContext
        {
            get { return Context; }
            set { Context = value; }
        }
        #endif

        [TestMethod]
        public void RegisteredAsDisposable_Test()
        {
            var container = new UnityContainer();
            container.AddNewExtension<DisposableStrategyExtension>();
            container.RegisterType<DisposableClass>(new DisposingTransientLifetimeManager());

            var obj1 = container.Resolve<DisposableClass>();
            var obj2 = container.Resolve<DisposableClass>();
            Assert.AreNotEqual(obj1, obj2);

            container.Teardown(obj1);
            Assert.IsTrue(obj1.Disposed);

            container.Teardown(obj2);
            Assert.IsTrue(obj2.Disposed);
        }

        /// <summary>
        ///     Checks if the default Unity behavior is unchanged: no disposal on Teardown.
        /// </summary>
        [TestMethod]
        public void UnregisteredClass_Test()
        {
            var container = new UnityContainer();
            container.AddNewExtension<DisposableStrategyExtension>();

            var obj1 = container.Resolve<DisposableClass>();
            var obj2 = container.Resolve<DisposableClass>();
            Assert.AreNotEqual(obj1, obj2);

            container.Teardown(obj1);
            Assert.IsFalse(obj1.Disposed);

            container.Teardown(obj2);
            Assert.IsFalse(obj2.Disposed);
        }

        [TestMethod]
        public void MixedRegistrations_Test()
        {
            var container = new UnityContainer();
            container.AddNewExtension<DisposableStrategyExtension>();
            container.RegisterType<DisposableClass>(new DisposingTransientLifetimeManager());
            container.RegisterType<DisposableClass>("transient");

            var disposingObj = container.Resolve<DisposableClass>();
            var disposingObj2 = container.Resolve<DisposableClass>("disposing");
            var transientObj = container.Resolve<DisposableClass>("transient");

            Assert.AreNotEqual(disposingObj, disposingObj2);
            Assert.AreNotEqual(disposingObj, transientObj);

            container.Teardown(disposingObj);
            Assert.IsTrue(disposingObj.Disposed);

            container.Teardown(disposingObj2);
            Assert.IsTrue(disposingObj.Disposed);

            container.Teardown(transientObj);
            Assert.IsFalse(transientObj.Disposed);
        }

        [TestMethod]
        public void MassDisposable_Test()
        {
            var container = new UnityContainer();
            container.AddNewExtension<DisposableStrategyExtension>();
            container.RegisterType<DisposableClass>(new DisposingTransientLifetimeManager());

            var sw = new Stopwatch();
            sw.Start();
            var objects = new List<DisposableClass>();
            for (int i = 0; i < 10000; i++)
            {
                objects.Add(container.Resolve<DisposableClass>());
            }

            sw.Stop();
            Context.WriteLine("Resolve: " + sw.Elapsed.ToString());

            sw.Restart();
            for (int i = 0; i < 10000; i++)
            {
                container.Teardown(objects[i]);
                Assert.IsTrue(objects[i].Disposed);
            }

            sw.Stop();
            Context.WriteLine("Teardown: " + sw.Elapsed.ToString());
        }

        [TestMethod]
        public void UnregisteredGenericParent_Test()
        {
            var container = new UnityContainer();
            container.AddNewExtension<DisposableStrategyExtension>();

            var obj1 = container.Resolve<GenericClass<SimpleClass>>();
            var obj2 = container.Resolve<GenericClass<DisposableClass>>();
            Assert.AreNotEqual(obj1, obj2);

            container.Teardown(obj1);
            container.Teardown(obj2);
            Assert.IsFalse(obj2.Property.Disposed);
        }

        [TestMethod]
        public void RegisteredParent_Test()
        {
            var container = new UnityContainer();
            container.AddNewExtension<DisposableStrategyExtension>();

            container.RegisterType<ClassWithDisposable>(new DisposingTransientLifetimeManager());

            var obj1 = container.Resolve<ClassWithDisposable>();
            container.Teardown(obj1);
            Assert.IsFalse(obj1.Disposable.Disposed);
        }

        [TestMethod]
        public void RegisteredParentAndChild_Test()
        {
            var container = new UnityContainer();
            container.AddNewExtension<DisposableStrategyExtension>();

            container.RegisterType<ClassWithDisposable>(new DisposingTransientLifetimeManager());
            container.RegisterType<DisposableClass>(new DisposingTransientLifetimeManager());

            var obj1 = container.Resolve<ClassWithDisposable>();
            container.Teardown(obj1);
            Assert.IsTrue(obj1.Disposable.Disposed);
        }

        /// <summary>
        /// Tests whether objects are disposed when their container is.
        /// </summary>
        [TestMethod]
        public void ContainerDisposed_Test()
        {
            DisposableClass obj1, obj2;

            using (var container = new UnityContainer())
            {
                container.AddNewExtension<DisposableStrategyExtension>();
                container.RegisterType<DisposableClass>(new DisposingTransientLifetimeManager());

                obj1 = container.Resolve<DisposableClass>();
                obj2 = container.Resolve<DisposableClass>();
            }

            Assert.IsTrue(obj1.Disposed);
            Assert.IsTrue(obj2.Disposed);
        }

        [TestMethod]
        public void NonDisposable_Test()
        {
            using (var container = new UnityContainer())
            {
                container.AddNewExtension<DisposableStrategyExtension>();
                container.RegisterType<SimpleClass>(new DisposingTransientLifetimeManager());

                var obj1 = container.Resolve<SimpleClass>();
                container.Teardown(obj1);
            }
        }
    }
}