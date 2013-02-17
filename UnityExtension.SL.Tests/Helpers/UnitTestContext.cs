using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnityExtension.Tests.Helpers
{
    public class UnitTestContext : TestContext
    {
        public override DbConnection DataConnection
        {
            get { throw new NotImplementedException(); }
        }

        public override DataRow DataRow
        {
            get { throw new NotImplementedException(); }
        }

        public override IDictionary Properties
        {
            get { throw new NotImplementedException(); }
        }

        public override void AddResultFile(string fileName)
        {
            throw new NotImplementedException();
        }

        public override void BeginTimer(string timerName)
        {
            throw new NotImplementedException();
        }

        public override void EndTimer(string timerName)
        {
            throw new NotImplementedException();
        }

        public override void WriteLine(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }
    }
}