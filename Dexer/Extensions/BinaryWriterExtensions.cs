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
using Dexer.IO;
using Dexer.IO.Markers;
using System.Collections.Generic;

namespace Dexer.Extensions
{
    public static class BinaryWriterExtensions
    {
        public static void EnsureAlignment(this BinaryWriter writer, int alignment, Action action)
        {
            long position = writer.BaseStream.Position;

            action();

            while ((writer.BaseStream.Position - position) % alignment != 0)
                writer.Write((byte)0);
        }

        public static void PreserveCurrentPosition(this BinaryWriter writer, uint newPosition, Action action)
        {
            long position = writer.BaseStream.Position;
            writer.BaseStream.Seek(newPosition, SeekOrigin.Begin);

            action();

            writer.BaseStream.Seek(position, SeekOrigin.Begin);
        }

        internal static UShortMarker MarkUShort(this BinaryWriter writer)
        {
            return new UShortMarker(writer); ;
        }

        internal static UIntMarker MarkUInt(this BinaryWriter writer)
        {
            return new UIntMarker(writer); ;
        }

        internal static SizeOffsetMarker MarkSizeOffset(this BinaryWriter writer)
        {
            return new SizeOffsetMarker(writer);
        }

        internal static SignatureMarker MarkSignature(this BinaryWriter writer)
        {
            return new SignatureMarker(writer);
        }

        public static void WriteULEB128(this BinaryWriter writer, uint value)
        {
            byte partial;
            do
            {
                partial = (byte)(value & 0x7f);
                value >>= 7;
                if (value != 0) 
                    partial |= 0x80;
                writer.Write(partial);
            } while (value != 0);
        }

        public static void WriteULEB128p1(this BinaryWriter writer, long value)
        {
            WriteULEB128(writer, (uint)(value + 1));
        }

        public static void WriteSLEB128(this BinaryWriter writer, int value)
        {

            bool negative = (value < 0);
            byte partial;
            bool next = true;

            while (next)
            {
                partial = (byte)(value & 0x7f);
                value >>= 7;
                if (negative)
                    value |= -(1 << 24);

                if ((value == 0 && ((partial & 0x40) == 0)) || (value == -1 && ((partial & 0x40) != 0)))
                    next = false;
                else
                    partial |= 0x80;

                writer.Write(partial);
            }
        }

        public static void WriteMUTF8String(this BinaryWriter writer, String value)
        {
            writer.WriteULEB128((uint)value.Length);

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if ((c != 0) && (c < 0x80))
                {
                    writer.Write((byte)c);
                }
                else if (c < 0x800)
                {
                    writer.Write((byte)(((c >> 6) & 0x1f) | 0xc0));
                    writer.Write((byte)((c & 0x3f) | 0x80));
                }
                else
                {
                    writer.Write((byte)(((c >> 12) & 0x0f) | 0xe0));
                    writer.Write((byte)(((c >> 6) & 0x3f) | 0xc0));
                    writer.Write((byte)((c & 0x3f) | 0x80));
                }
            }

            writer.Write((byte) 0); // 0 padded;
        }

        public static int GetBytesNeeded(this BinaryWriter writer, long value)
        {
            ulong tmp = (ulong)value;
            int result = 0;

            do
            {
                tmp >>= 8;
                result++;
            } while (tmp > 0);

            return result;
        }

        public static void WriteByByteLength(this BinaryWriter writer, long value, int byteLength)
        {
            for (int i = 0; i < byteLength; i++)
            {
                writer.Write((byte)(value & 0xFF));
                value >>= 8;
            }
        }

    }
}
