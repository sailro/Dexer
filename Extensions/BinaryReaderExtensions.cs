/*
    Dexer, open source framework for .DEX files (Dalvik Executable Format)
    Copyright (C) 2010 Sebastien LEBRETON

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.IO;

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

        public static uint ReadULEB128(this BinaryReader reader, out int shiftCount)
        {
            uint value = 0;
            bool hasNext = true;
            shiftCount = 0;

            while (hasNext)
            {
                uint data = reader.ReadByte();
                value |= (data & 0x7F) << shiftCount;
                shiftCount += 7;
                hasNext = (data & 0x80) != 0;
            }
            return value;
        }

        public static long ReadULEB128p1(this BinaryReader reader)
        {
            return ((long) ReadULEB128(reader)) - 1;
        }

        public static uint ReadULEB128(this BinaryReader reader)
        {
            int shiftCount;
            return ReadULEB128(reader, out shiftCount);
        }

        public static int ReadSLEB128(this BinaryReader reader)
        {
            int shiftCount;
            int value = (int)ReadULEB128(reader, out shiftCount);
            return (value << (32 - shiftCount)) >> (32 - shiftCount);
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

        public static long ReadValueByTypeArgument(this BinaryReader reader, int typeArgument)
        {
            return ReadSigned(reader, typeArgument + 1);
        }

        public static long ReadSigned(this BinaryReader reader, int byteLength)
        {
            long value = 0;
            for (int i = 0; i < byteLength; i++)
            {
                value |= (long)reader.ReadByte() << (8 * i);
            }
            int shift = 8 * byteLength;
            return (value << shift) >> shift;
        }

    }
}
