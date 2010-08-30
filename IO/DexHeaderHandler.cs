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

using System.IO;
using Dexer.Core;
using Dexer.Extensions;

namespace Dexer.IO
{
    internal class DexHeaderHandler : IBinaryReadable
    {

        private DexHeader Item { get; set; }

        public DexHeaderHandler(DexHeader item)
        {
            Item = item;
        }

        public void ReadFrom(BinaryReader reader)
        {
            Item.Magic = reader.ReadBytes(8);
            FormatChecker.CheckExpression(() => Item.Magic.Match(DexConsts.DexFileMagic, 0));

            Item.CheckSum = reader.ReadUInt32();
            Item.Signature = reader.ReadBytes(20);

            Item.FileSize = reader.ReadUInt32();
            Item.HeaderSize = reader.ReadUInt32();
            Item.EndianTag = reader.ReadUInt32();
            FormatChecker.CheckExpression(() => Item.EndianTag == DexConsts.Endian);

            Item.LinkSize = reader.ReadUInt32();
            Item.LinkOffset = reader.ReadUInt32();

            Item.MapOffset = reader.ReadUInt32();

            Item.StringsSize = reader.ReadUInt32();
            Item.StringsOffset = reader.ReadUInt32();

            Item.TypeReferencesSize = reader.ReadUInt32();
            Item.TypeReferencesOffset = reader.ReadUInt32();

            Item.PrototypesSize = reader.ReadUInt32();
            Item.PrototypesOffset = reader.ReadUInt32();

            Item.FieldReferencesSize = reader.ReadUInt32();
            Item.FieldReferencesOffset = reader.ReadUInt32();

            Item.MethodReferencesSize = reader.ReadUInt32();
            Item.MethodReferencesOffset = reader.ReadUInt32();

            Item.ClassDefinitionsSize = reader.ReadUInt32();
            Item.ClassDefinitionsOffset = reader.ReadUInt32();

            Item.DataSize = reader.ReadUInt32();
            Item.DataOffset = reader.ReadUInt32();
        }
    }
}
