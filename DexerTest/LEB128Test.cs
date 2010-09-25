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

namespace Dexer.Test
{
    [TestClass]
    public class LEB128Test : BaseTest
    {
        public void TestNumber<T>(Func<BinaryReader, T> readProvider, Action<BinaryWriter, T> writeProvider)
        {
            long number = 1;
            const int range = 16;
            MemoryStream stream = new MemoryStream();
            BinaryReader reader = new BinaryReader(stream);
            BinaryWriter writer = new BinaryWriter(stream);

            while (number <= ((long)uint.MaxValue) + 1)
            {
                for (int signed = 0; signed < 2; signed++)
                {
                    for (int i = -range; i <= range; i++)
                    {
                        try
                        {
                            long expectedlong = number + i;
                            if (signed!=0)
                                expectedlong = -1 * expectedlong;

                            T expected = (T)Convert.ChangeType(expectedlong, typeof(T));
                            T actual;

                            stream.Position = 0;
                            writeProvider(writer, expected);

                            stream.Position = 0;
                            actual = readProvider(reader);

                            Assert.AreEqual(expected, actual);
                        }
                        catch (OverflowException) { }
                    }
                }
                number <<= 1;
            }
        }
        
        [TestMethod]
        public void TestULEB128()
        {
            TestNumber<uint>((reader) => reader.ReadULEB128(), (writer, value) => writer.WriteULEB128(value));
        }

        [TestMethod]
        public void TestSLEB128()
        {
            TestNumber<int>((reader) => reader.ReadSLEB128(), (writer, value) => writer.WriteSLEB128(value));
        }
    }
}
