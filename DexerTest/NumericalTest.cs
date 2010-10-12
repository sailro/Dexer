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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Dexer.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dexer.Test
{
    [TestClass]
    public class NumericalTest : BaseTest
    {
        public void TestNumber<T>(Func<BinaryReader, T> readProvider, Action<BinaryWriter, T> writeProvider, IEnumerable<T> values)
        {
            MemoryStream stream = new MemoryStream();
            BinaryReader reader = new BinaryReader(stream);
            BinaryWriter writer = new BinaryWriter(stream);

            foreach (T item in values)
            {
                T expected = (T)Convert.ChangeType(item, typeof(T));
                T actual;

                TestContext.WriteLine("{0}, {0:x}", expected);

                stream.Position = 0;
                writeProvider(writer, expected);

                stream.Position = 0;
                actual = readProvider(reader);

                Assert.AreEqual(expected, actual);
            }

        }
        
        [TestMethod]
        public void TestULEB128()
        {
            TestNumber<uint>((reader) => reader.ReadULEB128(),
                             (writer, value) => writer.WriteULEB128(value),
                             GenerateUintValues());
        }

        [TestMethod]
        public void TestSLEB128()
        {
            TestNumber<int>((reader) => reader.ReadSLEB128(),
                            (writer, value) => writer.WriteSLEB128(value),
                            GenerateIntValues());
        }

        private int bytelength;
        
        private void VBLWriter(BinaryWriter writer, long value) {
            bytelength = writer.GetBytesNeeded(value);
            writer.WriteByByteLength(value, bytelength);
        }

        [TestMethod]
        public void TestVBL()
        {
            TestNumber<long>( (reader) => reader.ReadByByteLength(bytelength), 
                              VBLWriter,
                              GenerateLongValues());
        }

        private IEnumerable<long> GenerateLongValues()
        {
            foreach (long item1 in GenerateIntValues().AsParallel())
            {
                //foreach (long item2 in GenerateIntValues().AsParallel())
                //{
                    yield return item1 << 32; // | item2;
                //}
            }
        }

        private IEnumerable<uint> GenerateUintValues()
        {
            long value = 1;

            while (value <= uint.MaxValue)
            {
                yield return (uint)value - 1;
                yield return (uint)value;
                yield return (uint)value + 1;
                value = value * 2;
            }
        }

        private IEnumerable<int> GenerateIntValues()
        {
            foreach (uint item in GenerateUintValues())
            {
                yield return (int)item;
                yield return -(int)item;
            }
        }

    }
}
