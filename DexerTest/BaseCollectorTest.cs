/* Dexer Copyright (c) 2010-2013 Sebastien LEBRETON

Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the
"Software"), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. */

using System;
using System.Collections.Generic;
using System.IO;
using Dexer.Core;
using Dexer.IO.Collectors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dexer.Test
{
    [TestClass]
    public class BaseCollectorTest : BaseTest
    {
        internal void TestCollector<TC,T>(Func<Dex, List<T>> provider) where TC : BaseCollector<T>, new()
        {
            foreach (var file in Directory.GetFiles(FilesDirectory))
            {
                TestCollector<TC, T>(provider, file);
            }
        }

        internal TC TestCollector<TC, T>(Func<Dex, List<T>> provider, string file) where TC : BaseCollector<T>, new()
        {
            TestContext.WriteLine("Testing {0}", file);
            var dex = Dex.Read(file);

            var collector = new TC();
            collector.Collect(dex);

            foreach (var key in provider(dex))
                Assert.IsTrue(collector.Items.ContainsKey(key), "Item '{0}' not collected", key);

            foreach (var key in collector.Items.Keys)
                Assert.IsTrue(provider(dex).Contains(key), "Item '{0}' is 'over' collected", key);

            Assert.AreEqual(provider(dex).Count, collector.Items.Count);

            return collector;
        }

    }
}
