using System;

namespace UnityExtension.Tests.TestClasses
{
    public class GenericClass<T>
    {
        public GenericClass(T property)
        {
            Property = property;
        }

        public T Property { get; private set; }
    }
}