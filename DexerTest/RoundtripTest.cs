/* Dexer Copyright (c) 2010 Sebastien LEBRETON

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
using Dexer.Extensions;
using Dexer.IO.Collector;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dexer.IO;
using Dexer.Metadata;

namespace Dexer.Test
{
    [TestClass]
    public class RoundtripTest : BaseTest
    {

        [TestMethod]
        public void TestMap()
        {
            foreach (string file in Directory.GetFiles(FilesDirectory))
            {
                TestContext.WriteLine("Testing {0}", file);

                Dex dex = new Dex();
                DexReader dexreader = new DexReader(dex);

                using (Stream fs = new FileStream(file, FileMode.Open))
                    using (BinaryReader reader = new BinaryReader(fs))
                        dexreader.ReadFrom(reader);

                DexWriter dexwriter = new DexWriter(dex);
                dexwriter.WriteTo(new BinaryWriter(new MemoryStream()));

                foreach (TypeCodes tc in dexwriter.Map.Keys)
                {
                    if (dexreader.Map.ContainsKey(tc))
                    {
                        Assert.AreEqual(dexwriter.Map[tc].Offset, dexreader.Map[tc].Offset, "{0} Offset", tc);
                        Assert.AreEqual(dexwriter.Map[tc].Size, dexreader.Map[tc].Size, "{0} Offset", tc);
                    }
                }

            }
        }

    }
}
