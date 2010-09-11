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

namespace Dexer.Extensions
{
    public static class BinaryWriterExtensions
    {

        public static void PreserveCurrentPosition(this BinaryWriter writer, uint newPosition, Action action)
        {
            long position = writer.BaseStream.Position;
            writer.BaseStream.Seek(newPosition, SeekOrigin.Begin);

            action();

            writer.BaseStream.Seek(position, SeekOrigin.Begin);
        }

        public static UintMarker MarkUint(this BinaryWriter writer)
        {
            return new UintMarker(writer); ;
        }

        public static SizeOffsetMarker MarkSizeOffset(this BinaryWriter writer)
        {
            return new SizeOffsetMarker(writer);
        }

        public static SignatureMarker MarkSignature(this BinaryWriter writer)
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
            if (value < -1 || value >= uint.MaxValue)
                throw new ArgumentException("Allowed ULEB128p1 range is [-1, uint.MaxValue[");

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
                    writer.Write((byte)(((c >> 6) & 0x3f) | 0x80));
                    writer.Write((byte)((c & 0x3f) | 0x80));
                }
            }

            writer.Write((byte) 0); // 0 padded;
        }

    }
}
