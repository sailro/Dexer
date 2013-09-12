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
using System.IO;

namespace Dexer.Extensions
{
    public static class BinaryReaderExtensions
    {

        public static void PreserveCurrentPosition(this BinaryReader reader, uint newPosition, Action action)
        {
            var position = reader.BaseStream.Position;
            reader.BaseStream.Seek(newPosition, SeekOrigin.Begin);

            action();

            reader.BaseStream.Seek(position, SeekOrigin.Begin);
        }

        public static long ReadULEB128P1(this BinaryReader reader)
        {
            return ((long) ReadULEB128(reader)) - 1;
        }

        public static uint ReadULEB128(this BinaryReader reader)
        {
            var result = 0;
            var shift = 0;
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
            var result = 0;
            var shift = 0;
            byte partial;

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
            var stringLength = ReadULEB128(reader);
            var chars = new char[stringLength];
            for (int j = 0, jLength = chars.Length; j < jLength; j++)
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

        public static long ReadUnsignedPackedNumber(this BinaryReader reader, int byteLength)
        {
            long value = 0;
            for (var i = 0; i < byteLength; i++)
            {
                value |= (((long)(reader.ReadByte() & 0xFF)) << i * 8);
            }
            return value;
        }

        public static long ReadSignedPackedNumber(this BinaryReader reader, int byteLength)
        {
            long value = 0;
            for (var i = 0; i < byteLength; i++)
            {
                value |= (((long)((reader.ReadByte()) & 0xFF)) << (i * 8));
            }

            var shift = (8 - byteLength) * 8;
            return value << shift >> shift;
        }

    }
}
