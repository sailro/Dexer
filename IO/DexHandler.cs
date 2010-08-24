using System.Collections.Generic;
using System.IO;
using Dexer.Metadata;
using Dexer.Core;
using Dexer.Extensions;
using System;
using System.Collections;
using System.Diagnostics;

namespace Dexer.IO
{
    public class DexHandler : IBinaryReadable
    {
        private Dex Item { get; set; }

        private IList<string> Strings { get; set; }

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
        }

        public void ReadFrom(BinaryReader reader)
        {
            ReadHeader(reader);
            ReadMap(reader);
            ReadStrings(reader);

            PrefetchTypeReferences(reader);
            PrefetchClassDefinitions(reader, false);

            ReadTypesReferences(reader);
            ReadPrototypes(reader);
            ReadFieldReferences(reader);
            ReadMethodReferences(reader);

            PrefetchClassDefinitions(reader, true);
            ReadClassDefinitions(reader);
        }

        #region " Prefetch "
        private void PrefetchTypeReferences(BinaryReader reader)
        {
            reader.BaseStream.Seek(TypeReferencesOffset, SeekOrigin.Begin);
            for (int i = 0; i < TypeReferencesSize; i++)
            {
                int descriptorIndex = reader.ReadInt32();
                string descriptor = Strings[descriptorIndex];
                Item.TypeReferences.Add(TypeDescriptor.Allocate(descriptor));
            }
        }

        private void PrefetchFieldDefinitions(BinaryReader reader, ClassDefinition classDefinition, uint fieldcount)
        {
            int fieldIndex = 0;
            for (int i = 0; i < fieldcount; i++)
            {
                if (i == 0)
                {
                    fieldIndex = (int)reader.ReadULEB128();
                }
                else
                {
                    fieldIndex += (int)reader.ReadULEB128();
                }
                reader.ReadULEB128();
                FieldDefinition fdef = new FieldDefinition(Item.FieldReferences[fieldIndex]);
                Item.FieldReferences[fieldIndex] = fdef;
            }
        }

        private void PrefetchMethodDefinitions(BinaryReader reader, ClassDefinition classDefinition, uint methodcount)
        {
            int methodIndex = 0;
            for (int i = 0; i < methodcount; i++)
            {
                if (i == 0)
                {
                    methodIndex = (int)reader.ReadULEB128();
                }
                else
                {
                    methodIndex += (int)reader.ReadULEB128();
                }
                reader.ReadULEB128();
                reader.ReadULEB128();
                MethodDefinition mdef = new MethodDefinition(Item.MethodReferences[methodIndex]);
                Item.MethodReferences[methodIndex] = mdef;
            }
        }

        private void PrefetchClassDefinition(BinaryReader reader, ClassDefinition classDefinition, int classDataOffset)
        {
            long position = reader.BaseStream.Position;
            reader.BaseStream.Seek(classDataOffset, SeekOrigin.Begin);

            uint staticFieldSize = reader.ReadULEB128();
            uint instanceFieldSize = reader.ReadULEB128();
            uint directMethodSize = reader.ReadULEB128();
            uint virtualMethodSize = reader.ReadULEB128();

            PrefetchFieldDefinitions(reader, classDefinition, staticFieldSize);
            PrefetchFieldDefinitions(reader, classDefinition, instanceFieldSize);
            PrefetchMethodDefinitions(reader, classDefinition, directMethodSize);
            PrefetchMethodDefinitions(reader, classDefinition, virtualMethodSize);

            reader.BaseStream.Seek(position, SeekOrigin.Begin);
        }

        private void PrefetchClassDefinitions(BinaryReader reader, bool prefetchMembers)
        {
            reader.BaseStream.Seek(ClassDefinitionsOffset, SeekOrigin.Begin);
            for (int i = 0; i < ClassDefinitionsSize; i++)
            {
                int classIndex = reader.ReadInt32();

                ClassDefinition cdef;
                if (Item.TypeReferences[classIndex] is ClassDefinition)
                {
                    cdef = (ClassDefinition)Item.TypeReferences[classIndex];
                }
                else
                {
                    cdef = new ClassDefinition((ClassReference)Item.TypeReferences[classIndex]);
                    Item.TypeReferences[classIndex] = cdef;
                    Item.Classes.Add(cdef);
                }

                reader.ReadInt32(); // skip access_flags
                reader.ReadInt32(); // skip superclass_idx
                reader.ReadInt32(); // skip interfaces_off
                reader.ReadInt32(); // skip source_file_idx
                reader.ReadInt32(); // skip annotations_off

                int classDataOffset = reader.ReadInt32();
                if ((classDataOffset > 0) && prefetchMembers)
                {
                    PrefetchClassDefinition(reader, cdef, classDataOffset);
                }

                reader.ReadInt32(); // skip static_values_off
            }
        }
        #endregion

        #region " Read "
        private void ReadClassDefinitions(BinaryReader reader)
        {
            reader.BaseStream.Seek(ClassDefinitionsOffset, SeekOrigin.Begin);
            for (int i = 0; i < ClassDefinitionsSize; i++)
            {
                int classIndex = reader.ReadInt32();

                ClassDefinition cdef = (ClassDefinition)Item.TypeReferences[classIndex];
                cdef.AccessFlag = (AccessFlags)reader.ReadUInt32();

                int superClassIndex = reader.ReadInt32();
                cdef.SuperClass = (ClassReference)Item.TypeReferences[superClassIndex];

                int interfaceOffset = reader.ReadInt32();
                int sourceFileIndex = reader.ReadInt32();
                int annotationOffset = reader.ReadInt32();
                int classDataOffset = reader.ReadInt32();
                int staticValuesOffset = reader.ReadInt32();

                if (interfaceOffset > 0)
                    ReadInterfaces(reader, cdef, interfaceOffset);
                
                if (sourceFileIndex > 0)
                    cdef.SourceFile = Strings[sourceFileIndex];

                if (classDataOffset > 0)
                    ReadClassDefinition(reader, cdef, classDataOffset);

                if (annotationOffset > 0)
                    ReadAnnotationDirectory(reader, cdef, annotationOffset);

                if (staticValuesOffset > 0)
                {
                    long position = reader.BaseStream.Position;
                    reader.BaseStream.Seek(staticValuesOffset, SeekOrigin.Begin);

                    object[] values = ReadValues(reader);
                    for (int j = 0; j < values.Length; j++)
                    {
                        cdef.Fields[j].Value = values[j];
                    }
                    reader.BaseStream.Seek(position, SeekOrigin.Begin);
                }
            }
        }

        private void ReadInterfaces(BinaryReader reader, ClassDefinition classDefinition, int interfaceOffset)
        {
            long position = reader.BaseStream.Position;
            reader.BaseStream.Seek(interfaceOffset, SeekOrigin.Begin);

            int size = reader.ReadInt32();
            ushort index = reader.ReadUInt16();
            classDefinition.Interfaces.Add((ClassReference)Item.TypeReferences[index]);

            reader.BaseStream.Seek(position, SeekOrigin.Begin);
        }

        private void ReadAnnotationDirectory(BinaryReader reader, ClassDefinition classDefinition, int annotationOffset)
        {
            // TODO
        }

        private Annotation ReadEncodedAnnotation(BinaryReader reader)
        {
            int typeIndex = (int) reader.ReadULEB128();
            int elementSize = (int) reader.ReadULEB128();

            Annotation annotation = new Annotation();
            annotation.Type = (ClassReference) Item.TypeReferences[typeIndex];

            for (int i=0; i<elementSize; i++) {
                AnnotationArgument argument = new AnnotationArgument();
                int nameIndex = (int) reader.ReadULEB128();
                argument.Name = Strings[nameIndex];
                argument.Value = ReadValue(reader);
                annotation.Arguments.Add(argument);
            }

            return annotation;
        }

        private object[] ReadValues(BinaryReader reader)
        {
            uint size = reader.ReadULEB128();
            ArrayList array = new ArrayList();
            for (uint i = 0; i < size; i++)
            {
                array.Add(ReadValue(reader));
            }
            return array.ToArray();
        }

        private object ReadValue(BinaryReader reader)
        {
            int index;

            int data = reader.ReadByte();
            int valueFormat = data & 0x1F;
            int valueArgument = data >> 5;

            switch ((ValueFormats)valueFormat)
            {
                case ValueFormats.Byte:
                    return reader.ReadSByte();
                case ValueFormats.Short:
                    return (short) reader.ReadValueByTypeArgument(valueArgument);
                case ValueFormats.Char:
                    return (char)reader.ReadValueByTypeArgument(valueArgument);
                case ValueFormats.Int:
                    return (int)reader.ReadValueByTypeArgument(valueArgument);
                case ValueFormats.Long:
                    return (long)reader.ReadValueByTypeArgument(valueArgument);
                case ValueFormats.Float:
                    return reader.ReadSingle();
                case ValueFormats.Double:
                    return reader.ReadDouble();
                case ValueFormats.String:
                    return Strings[(int)reader.ReadValueByTypeArgument(valueArgument)];
                case ValueFormats.Type:
                    return Item.TypeReferences[(int)reader.ReadValueByTypeArgument(valueArgument)];
                case ValueFormats.Field:
                    return Item.TypeReferences[(int)reader.ReadValueByTypeArgument(valueArgument)];
                case ValueFormats.Method:
                    return Item.MethodReferences[(int)reader.ReadValueByTypeArgument(valueArgument)];
                case ValueFormats.Array:
                    return ReadValues(reader);
                case ValueFormats.Enum:       /* TODO */ throw new NotImplementedException();
                case ValueFormats.Annotation:
                    return ReadEncodedAnnotation(reader);
                case ValueFormats.Null:
                    return null;
                case ValueFormats.Boolean:
                    return reader.ReadBoolean();
                default:
                    throw new ArgumentException();
            }
        }

        private void ReadFieldDefinitions(BinaryReader reader, ClassDefinition classDefinition, uint fieldcount, Action<FieldDefinition> action)
        {
            int fieldIndex=0;
            for (int i = 0; i < fieldcount; i++)
            {
                if (i == 0)
                {
                    fieldIndex = (int)reader.ReadULEB128();
                }
                else
                {
                    fieldIndex += (int)reader.ReadULEB128();
                }

                uint accessFlags = reader.ReadULEB128();

                FieldDefinition fdef = (FieldDefinition)Item.FieldReferences[fieldIndex];
                fdef.AccessFlags = (AccessFlags)accessFlags;
                fdef.Owner = classDefinition;
                action(fdef);

                foreach (FieldDefinition test in classDefinition.Fields)
                {
                    Debug.Assert(!test.Equals(fdef));
                }
                classDefinition.Fields.Add(fdef);
            }
        }

        private void ReadMethodBody(BinaryReader reader, MethodDefinition methodDefinition, uint codeOffset)
        {

        }

        private void ReadMethodDefinitions(BinaryReader reader, ClassDefinition classDefinition, uint methodcount, Action<MethodDefinition> action)
        {
            int methodIndex = 0;
            for (int i = 0; i < methodcount; i++)
            {
                if (i == 0)
                {
                    methodIndex = (int)reader.ReadULEB128();
                }
                else
                {
                    methodIndex += (int)reader.ReadULEB128();
                }

                uint accessFlags = reader.ReadULEB128();
                uint codeOffset = reader.ReadULEB128();

                MethodDefinition mdef = (MethodDefinition)Item.MethodReferences[methodIndex];
                mdef.AccessFlags = (AccessFlags)accessFlags;
                mdef.Owner = classDefinition;
                action(mdef);

                ReadMethodBody(reader, mdef, codeOffset);

                classDefinition.Methods.Add(mdef);
            }
        }

        private void ReadClassDefinition(BinaryReader reader, ClassDefinition classDefinition, int classDataOffset)
        {
            long position = reader.BaseStream.Position;
            reader.BaseStream.Seek(classDataOffset, SeekOrigin.Begin);

            uint staticFieldSize = reader.ReadULEB128();
            uint instanceFieldSize = reader.ReadULEB128();
            uint directMethodSize = reader.ReadULEB128();
            uint virtualMethodSize = reader.ReadULEB128();

            ReadFieldDefinitions(reader, classDefinition, staticFieldSize, f => f.IsStatic = true);
            ReadFieldDefinitions(reader, classDefinition, instanceFieldSize, f => f.IsInstance = true);
            ReadMethodDefinitions(reader, classDefinition, directMethodSize, m => m.IsDirect = true);
            ReadMethodDefinitions(reader, classDefinition, virtualMethodSize, m => m.IsVirtual = true);

            reader.BaseStream.Seek(position, SeekOrigin.Begin);
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
                mref.Owner = (ClassReference)Item.TypeReferences[classIndex];
                mref.Prototype = Item.Prototypes[prototypeIndex];
                mref.Name = Strings[nameIndex];

                Item.MethodReferences.Add(mref);
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

                fref.Owner = (ClassReference)Item.TypeReferences[classIndex];
                fref.Type = Item.TypeReferences[typeIndex];
                fref.Name = Strings[nameIndex];

                Item.FieldReferences.Add(fref);
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
                prototype.ReturnType = Item.TypeReferences[returnTypeIndex];

                if (parametersOffset > 0)
                {
                    ReadParameters(reader, prototype, parametersOffset);
                }

                Item.Prototypes.Add(prototype);
            }
        }

        private void ReadParameters(BinaryReader reader, Prototype prototype, uint parametersOffset)
        {
            long position = reader.BaseStream.Position;
            reader.BaseStream.Seek(parametersOffset, SeekOrigin.Begin);

            uint typecount = reader.ReadUInt32();
            for (int j = 0; j < typecount; j++)
            {
                Parameter parameter = new Parameter();
                int typeIndex = reader.ReadUInt16();
                parameter.Type = Item.TypeReferences[typeIndex];
                prototype.Parameters.Add(parameter);
            }

            reader.BaseStream.Seek(position, SeekOrigin.Begin);
        }
        
        private void ReadTypesReferences(BinaryReader reader)
        {
            reader.BaseStream.Seek(TypeReferencesOffset, SeekOrigin.Begin);
            for (int i = 0; i < TypeReferencesSize; i++)
            {
                int descriptorIndex = reader.ReadInt32();
                string descriptor = Strings[descriptorIndex];
                TypeDescriptor.Fill(descriptor, Item.TypeReferences[i], Item);
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
        #endregion
    }
}
