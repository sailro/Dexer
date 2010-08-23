using System.Collections.Generic;
using System.IO;
using Dexer.Metadata;
using Dexer.Core;

namespace Dexer.IO
{
    public class DexHandler : IBinaryReadable
    {
        private Dex Item { get; set; }

        private IList<string> Strings { get; set; }
        private IList<TypeReference> TypeReferences { get; set; }
        private IList<Prototype> Prototypes { get; set; }
        private IList<FieldReference> FieldReferences { get; set; }
        private IList<MethodReference> MethodReferences { get; set; }

        private byte[] Magic { get; set; }
        private uint CheckSum { get; set; }
        private byte[] Signature { get; set; }

        private uint FileSize { get; set; }
        private uint HeaderSize { get; set; }
        private uint EndianTag { get; set; }

        private uint LinkSize { get; set; }
        private uint LinkOffset { get; set; }

        private uint MapOffset { get; set; }

        private uint StringsSize { get; set; }
        private uint StringsOffset { get; set; }

        private uint TypeReferencesSize { get; set; }
        private uint TypeReferencesOffset { get; set; }

        private uint PrototypesSize { get; set; }
        private uint PrototypesOffset { get; set; }

        private uint FieldReferencesSize { get; set; }
        private uint FieldReferencesOffset { get; set; }

        private uint MethodReferencesSize { get; set; }
        private uint MethodReferencesOffset { get; set; }

        private uint ClassDefinitionsSize { get; set; }
        private uint ClassDefinitionsOffset { get; set; }

        private uint DataSize { get; set; }
        private uint DataOffset { get; set; }

        private Map Map { get; set; }

        public DexHandler(Dex item)
        {
            Item = item;

            Map = new Map();
            Strings = new List<string>();
            TypeReferences = new List<TypeReference>();
            Prototypes = new List<Prototype>();
            FieldReferences = new List<FieldReference>();
            MethodReferences = new List<MethodReference>();
            Item.ClassDefinitions = new List<ClassDefinition>();
        }

        public void ReadFrom(BinaryReader reader)
        {
            ReadHeader(reader);
            ReadMap(reader);
            ReadStrings(reader);
            ReadTypesReferences(reader);
            ReadPrototypes(reader);
            ReadFieldReferences(reader);
            ReadMethodReferences(reader);
            ReadClassDefinitions(reader);

            FlushLoaders();
        }

        private void FlushLoaders()
        {
            foreach (var item in TypeReferences)
            {
                if (item is ClassReference)
                {
                    (item as ClassReference).FlushLoaders();
                }
            }
        }

        private void ReadClassDefinitions(BinaryReader reader)
        {
            reader.BaseStream.Seek(ClassDefinitionsOffset, SeekOrigin.Begin);
            for (int i = 0; i < ClassDefinitionsSize; i++)
            {
                int classIndex = reader.ReadInt32();

                ClassDefinition cdef = ((ClassReference) TypeReferences[classIndex]).Promote();
                TypeReferences[classIndex] = cdef;
                Item.ClassDefinitions.Add(cdef);

                cdef.AccessFlag = (AccessFlags)reader.ReadUInt32();

                int superClassIndex = reader.ReadInt32();
                cdef.DelayLoad(c => c.SuperClass = (ClassReference) TypeReferences[superClassIndex]);

                int interfaceOffset = reader.ReadInt32();
                if (interfaceOffset > 0)
                {
                    // TODO
                }

                int sourceFileIndex = reader.ReadInt32();
                if (sourceFileIndex > 0)
                {
                    cdef.SourceFile = Strings[sourceFileIndex];
                }

                int annotationOffset = reader.ReadInt32();
                if (annotationOffset > 0)
                {
                    // TODO
                }

                int classDataOffset = reader.ReadInt32();
                if (classDataOffset > 0)
                {
                    // TODO
                }

                int staticValuesOffset = reader.ReadInt32();
                if (staticValuesOffset > 0)
                {
                    // TODO
                }
            }
        }

        private void ReadMethodReferences(BinaryReader reader)
        {
            reader.BaseStream.Seek(MethodReferencesOffset, SeekOrigin.Begin);
            for (int i = 0; i < MethodReferencesSize; i++)
            {
                int classIndex = reader.ReadUInt16();
                int prototypeIndex = reader.ReadUInt16();
                int nameIndex = reader.ReadInt32();

                MethodReference mref = new MethodReference();
                mref.DelayLoad(m => m.ClassReference = (ClassReference)TypeReferences[classIndex]);
                mref.Prototype = Prototypes[prototypeIndex];
                mref.Name = Strings[nameIndex];

                MethodReferences.Add(mref);
            }
        }

        private void ReadFieldReferences(BinaryReader reader)
        {
            reader.BaseStream.Seek(FieldReferencesOffset, SeekOrigin.Begin);
            for (int i = 0; i < FieldReferencesSize; i++)
            {
                int classIndex = reader.ReadUInt16();
                int typeIndex = reader.ReadUInt16();
                int nameIndex = reader.ReadInt32();

                FieldReference fref = new FieldReference();

                fref.DelayLoad(f =>  f.ClassReference = (ClassReference) TypeReferences[classIndex]);
                fref.DelayLoad(f => fref.Type = TypeReferences[typeIndex]);
                fref.Name = Strings[nameIndex];

                FieldReferences.Add(fref);
            }
        }

        private void ReadPrototypes(BinaryReader reader)
        {
            reader.BaseStream.Seek(PrototypesOffset, SeekOrigin.Begin);
            for (int i = 0; i < PrototypesSize; i++)
            {
                int shortyIndex = reader.ReadInt32();
                int returnTypeIndex = reader.ReadInt32();
                uint parametersOffset = reader.ReadUInt32();

                Prototype prototype = new Prototype();
                prototype.DelayLoad(p => p.ReturnType = TypeReferences[returnTypeIndex]);

                if (parametersOffset > 0)
                {
                    long position = reader.BaseStream.Position;
                    reader.BaseStream.Seek(parametersOffset, SeekOrigin.Begin);

                    uint typecount = reader.ReadUInt32();
                    for (int j = 0; j < typecount; j++)
                    {
                        Parameter parameter = new Parameter();
                        int typeIndex = reader.ReadUInt16();
                        parameter.DelayLoad(p => p.Type = TypeReferences[typeIndex]);
                        prototype.Parameters.Add(parameter);
                    }

                    reader.BaseStream.Seek(position, SeekOrigin.Begin);
                }

                Prototypes.Add(prototype);
            }
        }
        
        private void ReadTypesReferences(BinaryReader reader)
        {
            reader.BaseStream.Seek(TypeReferencesOffset, SeekOrigin.Begin);
            for (int i = 0; i < TypeReferencesSize; i++)
            {
                int descriptorIndex = reader.ReadInt32();
                string descriptor = Strings[descriptorIndex];
                TypeReferences.Add(TypeDescriptor.Parse(descriptor));
            }
        }

        private void ReadStrings(BinaryReader reader)
        {
            reader.BaseStream.Seek(StringsOffset, SeekOrigin.Begin);
            uint StringsDataOffset = reader.ReadUInt32();
            reader.BaseStream.Seek(StringsDataOffset, SeekOrigin.Begin);
            for (int i = 0; i < StringsSize; i++)
            {
                Strings.Add(reader.ReadMUTF8String());
            }
        }

        private void ReadMap(BinaryReader reader)
        {
            reader.BaseStream.Seek(MapOffset, SeekOrigin.Begin);
            uint mapsize = reader.ReadUInt32();
            for (int i = 0; i < mapsize; i++)
            {
                MapItem item = new MapItem();
                item.Type = (TypeCodes)reader.ReadUInt16();
                reader.ReadUInt16(); // unused
                item.Size = reader.ReadUInt32();
                item.Offset = reader.ReadUInt32();
                Map.Add(item.Type, item);
            }
        }

        private void ReadHeader(BinaryReader reader)
        {
            Magic = reader.ReadBytes(8);
            CheckSum = reader.ReadUInt32();
            Signature = reader.ReadBytes(20);

            FileSize = reader.ReadUInt32();
            HeaderSize = reader.ReadUInt32();
            EndianTag = reader.ReadUInt32();

            LinkSize = reader.ReadUInt32();
            LinkOffset = reader.ReadUInt32();

            MapOffset = reader.ReadUInt32();

            StringsSize = reader.ReadUInt32();
            StringsOffset = reader.ReadUInt32();

            TypeReferencesSize = reader.ReadUInt32();
            TypeReferencesOffset = reader.ReadUInt32();

            PrototypesSize = reader.ReadUInt32();
            PrototypesOffset = reader.ReadUInt32();

            FieldReferencesSize = reader.ReadUInt32();
            FieldReferencesOffset = reader.ReadUInt32();

            MethodReferencesSize = reader.ReadUInt32();
            MethodReferencesOffset = reader.ReadUInt32();

            ClassDefinitionsSize = reader.ReadUInt32();
            ClassDefinitionsOffset = reader.ReadUInt32();

            DataSize = reader.ReadUInt32();
            DataOffset = reader.ReadUInt32();
        }
    }
}
