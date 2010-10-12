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
using System.IO;
using System.Diagnostics;

namespace Dexer.Extensions
{
    public static class BinaryReaderExtensions
    {

        public static void PreserveCurrentPosition(this BinaryReader reader, uint newPosition, Action action)
        {
            long position = reader.BaseStream.Position;
            reader.BaseStream.Seek(newPosition, SeekOrigin.Begin);

            action();

            reader.BaseStream.Seek(position, SeekOrigin.Begin);
        }

        public static long ReadULEB128p1(this BinaryReader reader)
        {
            return ((long) ReadULEB128(reader)) - 1;
        }

        public static uint ReadULEB128(this BinaryReader reader)
        {
            int result = 0;
            int shift = 0;
            byte partial;

            do
            {
                partial = reader.ReadByte();
                result |= (partial & 0x7f) << shift;
                shift += 7;
            } while ((partial & 0x80) != 0);

            return (uint)result;
        }

        public static int ReadSLEB128(this BinaryReader reader)
        {
            int result = 0;
            int shift = 0;
            byte partial = 0;

            do
            {
                partial = reader.ReadByte();
                result |= (partial & 0x7F) << shift;
                shift += 7;
            } while ((partial & 0x80) != 0);

            if ((shift < 31) && ((partial & 0x40) == 0x40))
                result |= -(1 << shift);

            return result;
        }

        public static String ReadMUTF8String(this BinaryReader reader)
        {
            uint stringLength = ReadULEB128(reader);
            char[] chars = new char[stringLength];
            for (int j = 0, j_length = chars.Length; j < j_length; j++)
            {
                int data = reader.ReadByte();
                switch (data >> 4)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                        chars[j] = (char)data;
                        break;
                    case 12:
                    case 13:
                        chars[j] = (char)(((data & 0x1F) << 6) | (reader.ReadByte() & 0x3F));
                        break;
                    case 14:
                        chars[j] = (char)(((data & 0x0F) << 12) | ((reader.ReadByte() & 0x3F) << 6) | (reader.ReadByte() & 0x3F));
                        break;
                    default:
                        throw new ArgumentException("illegal MUTF8 byte");
                }
            }
            reader.ReadByte(); // 0 padded;
            return new String(chars);
        }

        public static long ReadByByteLength(this BinaryReader reader, int byteLength)
        {
            ulong value = 0;
            long result;

            for (int i = 0; i < byteLength; i++)
            {
                value |= ((ulong)reader.ReadByte()) << (8 * i);
            }

            //int shift = 8 * byteLength;
            //result = (long)(value << shift) >> shift;
            result = (long) value;
            
            if (byteLength != BinaryWriterExtensions.GetBytesNeeded(null, result))
            {
                Console.WriteLine(string.Format("{0:x} {1} != {2}", result, byteLength, BinaryWriterExtensions.GetBytesNeeded(null, result)));
            }

            return result;
        }

    }
}
