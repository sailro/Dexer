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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Dexer.Core;
using Dexer.Extensions;
using Dexer.Metadata;

namespace Dexer.IO
{
    public class DexHandler : IBinaryReadable
    {
        readonly byte[] DexFileMagic = { 0x64, 0x65, 0x78, 0x0a, 0x30, 0x33, 0x35, 0x00 };
        const uint Endian = 0x12345678;
        const uint ReverseEndian = 0x78563412;
        const uint NoIndex = 0xffffffff;
        const char InnerClassMarker = '$';

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

            Hierarchicalize();
        }

        private void Hierarchicalize()
        {
            var TopClasses = new List<ClassDefinition>();
            foreach (var cdef in Item.Classes)
            {
                if (cdef.Fullname.Contains(InnerClassMarker.ToString()))
                {
                    String[] items = cdef.Fullname.Split(InnerClassMarker);
                    string fullname = items[0];
                    string name = items[1];
                    ClassDefinition owner = Item.GetClass(fullname);
                    if (owner != null)
                    {
                        owner.InnerClasses.Add(cdef);
                        cdef.Owner = owner;
                    }
                }
                else
                {
                    TopClasses.Add(cdef);
                }
            }
            Item.Classes = TopClasses;
        }

        private void PreserveCurrentPosition(BinaryReader reader, uint newPosition, Action action)
        {
            long position = reader.BaseStream.Position;
            reader.BaseStream.Seek(newPosition, SeekOrigin.Begin);

            action();

            reader.BaseStream.Seek(position, SeekOrigin.Begin);
        }

        #region " Prefetch "
        private void PrefetchTypeReferences(BinaryReader reader)
        {
            PreserveCurrentPosition(reader, TypeReferencesOffset, () =>
            {
                reader.BaseStream.Seek(TypeReferencesOffset, SeekOrigin.Begin);

                for (int i = 0; i < TypeReferencesSize; i++)
                {
                    int descriptorIndex = reader.ReadInt32();
                    string descriptor = Strings[descriptorIndex];
                    Item.TypeReferences.Add(TypeDescriptor.Allocate(descriptor));
                }

            });
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

        private void PrefetchClassDefinition(BinaryReader reader, ClassDefinition classDefinition, uint classDataOffset)
        {
            PreserveCurrentPosition(reader, classDataOffset, () =>
            {
                uint staticFieldSize = reader.ReadULEB128();
                uint instanceFieldSize = reader.ReadULEB128();
                uint directMethodSize = reader.ReadULEB128();
                uint virtualMethodSize = reader.ReadULEB128();

                PrefetchFieldDefinitions(reader, classDefinition, staticFieldSize);
                PrefetchFieldDefinitions(reader, classDefinition, instanceFieldSize);
                PrefetchMethodDefinitions(reader, classDefinition, directMethodSize);
                PrefetchMethodDefinitions(reader, classDefinition, virtualMethodSize);
            });
        }

        private void PrefetchClassDefinitions(BinaryReader reader, bool prefetchMembers)
        {
            PreserveCurrentPosition(reader, ClassDefinitionsOffset, () =>
            {
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

                    uint classDataOffset = reader.ReadUInt32();
                    if ((classDataOffset > 0) && prefetchMembers)
                    {
                        PrefetchClassDefinition(reader, cdef, classDataOffset);
                    }

                    reader.ReadInt32(); // skip static_values_off
                }
            });
        }
        #endregion

        #region " Read "
        private void ReadClassDefinitions(BinaryReader reader)
        {
            PreserveCurrentPosition(reader, ClassDefinitionsOffset, () =>
            {
                for (int i = 0; i < ClassDefinitionsSize; i++)
                {
                    uint classIndex = reader.ReadUInt32();

                    ClassDefinition cdef = (ClassDefinition)Item.TypeReferences[(int)classIndex];
                    cdef.AccessFlag = (AccessFlags)reader.ReadUInt32();

                    uint superClassIndex = reader.ReadUInt32();
                    if (superClassIndex != NoIndex)
                        cdef.SuperClass = (ClassReference)Item.TypeReferences[(int)superClassIndex];

                    uint interfaceOffset = reader.ReadUInt32();
                    uint sourceFileIndex = reader.ReadUInt32();
                    uint annotationOffset = reader.ReadUInt32();
                    uint classDataOffset = reader.ReadUInt32();
                    uint staticValuesOffset = reader.ReadUInt32();

                    if (interfaceOffset > 0)
                        ReadInterfaces(reader, cdef, interfaceOffset);

                    if (sourceFileIndex != NoIndex)
                        cdef.SourceFile = Strings[(int)sourceFileIndex];

                    if (classDataOffset > 0)
                        ReadClassDefinition(reader, cdef, classDataOffset);

                    if (annotationOffset > 0)
                        ReadAnnotationDirectory(reader, cdef, annotationOffset);

                    if (staticValuesOffset > 0)
                    {
                        ReadStaticValues(reader, cdef, staticValuesOffset);
                    }
                }
            });
        }

        private void ReadStaticValues(BinaryReader reader, ClassDefinition classDefinition, uint staticValuesOffset)
        {
            PreserveCurrentPosition(reader, staticValuesOffset, () =>
            {
                object[] values = ReadValues(reader);
                for (int j = 0; j < values.Length; j++)
                {
                    classDefinition.Fields[j].Value = values[j];
                }
            });
        }

        private void ReadInterfaces(BinaryReader reader, ClassDefinition classDefinition, uint interfaceOffset)
        {
            PreserveCurrentPosition(reader, interfaceOffset, () =>
            {
                int size = reader.ReadInt32();
                ushort index = reader.ReadUInt16();
                classDefinition.Interfaces.Add((ClassReference)Item.TypeReferences[index]);
            });
        }

        private void ReadAnnotationDirectory(BinaryReader reader, ClassDefinition classDefinition, uint annotationOffset)
        {
            PreserveCurrentPosition(reader, annotationOffset, () =>
            {
                uint classAnnotationOffset = reader.ReadUInt32();
                uint annotatedFieldsSize = reader.ReadUInt32();
                uint annotatedMethodsSize = reader.ReadUInt32();
                uint annotatedParametersSize = reader.ReadUInt32();

                if (classAnnotationOffset > 0)
                    classDefinition.Annotations = ReadAnnotationSet(reader, classAnnotationOffset);

                if (annotatedFieldsSize > 0)
                    (Item.FieldReferences[reader.ReadInt32()] as FieldDefinition).Annotations = ReadAnnotationSet(reader, reader.ReadUInt32());

                if (annotatedMethodsSize > 0)
                    (Item.MethodReferences[reader.ReadInt32()] as MethodDefinition).Annotations = ReadAnnotationSet(reader, reader.ReadUInt32());

                if (annotatedParametersSize > 0)
                {
                    int methodIndex = reader.ReadInt32();
                    uint offset = reader.ReadUInt32();
                    var annotations = ReadAnnotationSetRefList(reader, offset);
                    MethodDefinition mdef = (Item.MethodReferences[methodIndex] as MethodDefinition);

                    for (int i = 0; i < annotations.Count; i++)
                    {
                        if (annotations[i].Count > 0)
                        {
                            AnnotatedParameter aprm = new AnnotatedParameter();
                            aprm.Parameter = mdef.Prototype.Parameters[i];
                            aprm.Annotations = annotations[i];
                            mdef.AnnotatedParameters.Add(aprm);
                        }
                    }
                }
            });
        }

        private IList<IList<Annotation>> ReadAnnotationSetRefList(BinaryReader reader, uint annotationOffset)
        {
            var result = new List<IList<Annotation>>();
            PreserveCurrentPosition(reader, annotationOffset, () =>
            {
                uint size = reader.ReadUInt32();
                for (uint i = 0; i < size; i++)
                {
                    uint offset = reader.ReadUInt32();
                    result.Add(ReadAnnotationSet(reader, offset));
                }
            });
            return result;
        }

        private IList<Annotation> ReadAnnotationSet(BinaryReader reader, uint annotationOffset)
        {
            var result = new List<Annotation>();
            PreserveCurrentPosition(reader, annotationOffset, () =>
            {
                uint size = reader.ReadUInt32();
                for (uint i = 0; i < size; i++)
                {
                    uint offset = reader.ReadUInt32();
                    result.Add(ReadAnnotation(reader, offset));
                }
            });
            return result;
        }

        private Annotation ReadAnnotation(BinaryReader reader, uint annotationOffset)
        {
            Annotation annotation = null;
            PreserveCurrentPosition(reader, annotationOffset, () =>
            {
                byte visibility = reader.ReadByte();
                annotation = ReadEncodedAnnotation(reader);
                annotation.Visibility = (AnnotationVisibility)visibility;
            });
            return annotation;
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
                case ValueFormats.Enum:       
                    return Item.FieldReferences[(int)reader.ReadValueByTypeArgument(valueArgument)];
                case ValueFormats.Method:
                    return Item.MethodReferences[(int)reader.ReadValueByTypeArgument(valueArgument)];
                case ValueFormats.Array:
                    return ReadValues(reader);
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

        private void ReadFieldDefinitions(BinaryReader reader, ClassDefinition classDefinition, uint fieldcount)
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

                classDefinition.Fields.Add(fdef);
            }
        }

        private void ReadMethodDefinitions(BinaryReader reader, ClassDefinition classDefinition, uint methodcount)
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

                classDefinition.Methods.Add(mdef);

                ReadMethodBody(reader, mdef, codeOffset);
            }
        }

        private void ReadMethodBody(BinaryReader reader, MethodDefinition methodDefinition, uint codeOffset)
        {
            PreserveCurrentPosition(reader, codeOffset, () =>
            {
                ushort registersSize = reader.ReadUInt16();
                ushort incomingArgsSize = reader.ReadUInt16();
                ushort outgoingArgsSize = reader.ReadUInt16();
                ushort triesSize = reader.ReadUInt16();
                uint debugOffset = reader.ReadUInt32();

                ReadInstructions(reader, methodDefinition);

                //if ((triesSize != 0) && (instructionsSize % 2 != 0))
                  //  reader.ReadUInt16(); // padding (4-byte alignment)

                /*            
                tries 	try_item[tries_size] (optional) 	array indicating where in the code exceptions may be caught and how to handle them. Elements of the array must be non-overlapping in range and in order from low to high address. This element is only present if tries_size is non-zero.
                handlers 	encoded_catch_handler_list (optional) 	bytes representing a list of lists of catch types and associated handler addresses. Each try_item has a byte-wise offset into this structure. This element is only present if tries_size is non-zero. 
                 */


            });

        }

        private void ReadInstructions(BinaryReader reader, MethodDefinition methodDefinition)
        {
            var instructions = new List<Instruction>();
            uint instructionsSize = reader.ReadUInt32();
            var registers = new Register[256];

            int[] codes = new int[instructionsSize];
            int[] lower = new int[instructionsSize];
            int[] upper = new int[instructionsSize];

            for (int i = 0; i < instructionsSize; i++)
            {
                codes[i] = reader.ReadUInt16();
                lower[i] = codes[i] & 0xFF;
                upper[i] = codes[i] >> 8;
            }

            int ip = 0;
            while (ip < instructionsSize)
            {
                Instruction ins = new Instruction();
                instructions.Add(ins);

                ins.OpCode = (OpCodes)lower[ip];

                int dri = 0;
                int sri = 0;
                int data;
                object value;

                switch (ins.OpCode)
                {

                    case OpCodes.Nop:
                    case OpCodes.Return_void:
                            // [null]
                            ip++;
                            break;
                    case OpCodes.Move_result:
                    case OpCodes.Move_result_wide:
                    case OpCodes.Move_result_object:
                    case OpCodes.Move_exception:
                    case OpCodes.Return:
                    case OpCodes.Return_wide:
                    case OpCodes.Return_object:
					case OpCodes.Monitor_enter: 
					case OpCodes.Monitor_exit: 
					case OpCodes.Throw:
                            // vAA
                            dri = upper[ip++];
                            break;
                    case OpCodes.Move_object:
                    case OpCodes.Move_wide:
                    case OpCodes.Move:
  					case OpCodes.Array_length: 
                            // vA, vB
                            dri = upper[ip] & 0xF;
                            sri = upper[ip++] >> 4;
                            break;
                    case OpCodes.Move_wide_from16:
                    case OpCodes.Move_from16:
                    case OpCodes.Move_object_from16:
                            // vAA, vBBBB
                            dri = upper[ip++];
                            sri = codes[ip++];
                            break;
                    case OpCodes.Move_16:
                    case OpCodes.Move_object_16:
                            // vAAAA, vBBBB
                            ip++;
                            dri = codes[ip++];
                            sri = codes[ip++];
                            break;
                    case OpCodes.Const_4:
                            // vA, #+B
                            dri = upper[ip] & 0xF;
                            value = (upper[ip++] << 24) >> 28;
                            break;
                    case OpCodes.Const_16:
                    case OpCodes.Const_wide_16:
                            // vAA, #+BBBB
                            dri = upper[ip++];
                            value = codes[ip++];
                            break;
                    case OpCodes.Const:
                    case OpCodes.Const_wide_32:
                    case OpCodes.Fill_array_data:
                            // vAA, #+BBBBBBBB
                            dri = upper[ip++];
                            value = codes[ip++];
                            value = (int) value | codes[ip++] << 16;
                            break;
                    case OpCodes.Const_high16:
                            // vAA, #+BBBB0000
                            dri = upper[ip++];
                            value = codes[ip++] << 16;
                            break;
                    case OpCodes.Const_wide:
                            // vAA, #+BBBBBBBBBBBBBBBB
                            dri = upper[ip++];
                            value = codes[ip++];
                            value = (long) value | (long)codes[ip++] << 16;
                            value = (long) value | (long)codes[ip++] << 32;
                            value = (long) value | (long)codes[ip++] << 48;
                            break;
                    case OpCodes.Const_wide_high16:
                            // vAA, #+BBBB000000000000
                            dri = upper[ip++];
                            value = (long)codes[ip++] << 48;
                            break;
                    case OpCodes.Const_string:
                            // vAA, string@BBBB
                            dri = upper[ip++];
                            value = Strings[codes[ip++]];
                            break;
                    case OpCodes.Const_string_jumbo:
                            // vAA, string@BBBBBBBB
                            dri = upper[ip++];
                            data = codes[ip++];
                            data |= codes[ip++] << 16;
                            value = Strings[data];
                            break;
                    case OpCodes.Const_class:
                    case OpCodes.New_instance:
					case OpCodes.Check_cast: 
                            //  vAA, type@BBBB
                            dri = upper[ip++];
                            value = Item.TypeReferences[codes[ip++]];
                            break;
					case OpCodes.Instance_of: 
					case OpCodes.New_array: 
						    // vA, vB, type@CCCC
     						dri = upper[ip] & 0xF;
	     					sri = upper[ip++] >> 4;
                            value = Item.TypeReferences[codes[ip++]];
			     			break;
					case OpCodes.Filled_new_array: 
						    // {vD, vE, vF, vG, vA}, type@CCCC
						    throw new NotImplementedException(); // TODO Implement
					case OpCodes.Filled_new_array_range: 
						    // {vCCCC .. vNNNN}, type@BBBB
						    throw new NotImplementedException(); // TODO Implement

                        
					/*case OpCodes.Goto: {
						// +AA
						value = (byte)upper[ip++];
						frame.pc += offset - 1;
						break;
					}
					case OpCodes.Goto_16: {
						// goto/16 +AAAA
						frame.pc++;
						int offset = (short)codes[frame.pc++];
						frame.pc += offset - 2;
						break;
					}
					case OpCodes.Goto_32: {
						// goto/32 +AAAAAAAA
						frame.pc++;
						int offset = codes[frame.pc++];
						offset |= codes[frame.pc++] << 16;
						frame.pc += offset - 3;
						break;
					}*/



                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private void ReadClassDefinition(BinaryReader reader, ClassDefinition classDefinition, uint classDataOffset)
        {
            PreserveCurrentPosition(reader, classDataOffset, () =>
            {
                uint staticFieldSize = reader.ReadULEB128();
                uint instanceFieldSize = reader.ReadULEB128();
                uint directMethodSize = reader.ReadULEB128();
                uint virtualMethodSize = reader.ReadULEB128();

                ReadFieldDefinitions(reader, classDefinition, staticFieldSize);
                ReadFieldDefinitions(reader, classDefinition, instanceFieldSize);
                ReadMethodDefinitions(reader, classDefinition, directMethodSize);
                ReadMethodDefinitions(reader, classDefinition, virtualMethodSize);
            });
        }

        private void ReadMethodReferences(BinaryReader reader)
        {
            PreserveCurrentPosition(reader, MethodReferencesOffset, () =>
            {
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
            });
        }

        private void ReadFieldReferences(BinaryReader reader)
        {
            PreserveCurrentPosition(reader, FieldReferencesOffset, () =>
            {
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
            });
        }

        private void ReadPrototypes(BinaryReader reader)
        {
            PreserveCurrentPosition(reader, PrototypesOffset, () =>
            {
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
            });
        }

        private void ReadParameters(BinaryReader reader, Prototype prototype, uint parametersOffset)
        {
            PreserveCurrentPosition(reader, parametersOffset, () =>
            {
                uint typecount = reader.ReadUInt32();
                for (int j = 0; j < typecount; j++)
                {
                    Parameter parameter = new Parameter();
                    int typeIndex = reader.ReadUInt16();
                    parameter.Type = Item.TypeReferences[typeIndex];
                    prototype.Parameters.Add(parameter);
                }
            });
        }
        
        private void ReadTypesReferences(BinaryReader reader)
        {
            PreserveCurrentPosition(reader, TypeReferencesOffset, () =>
            {
                for (int i = 0; i < TypeReferencesSize; i++)
                {
                    int descriptorIndex = reader.ReadInt32();
                    string descriptor = Strings[descriptorIndex];
                    TypeDescriptor.Fill(descriptor, Item.TypeReferences[i], Item);
                }
            });
        }

        private void ReadStrings(BinaryReader reader)
        {
            PreserveCurrentPosition(reader, StringsOffset, () =>
            {
                uint StringsDataOffset = reader.ReadUInt32();
                reader.BaseStream.Seek(StringsDataOffset, SeekOrigin.Begin);
                for (int i = 0; i < StringsSize; i++)
                {
                    Strings.Add(reader.ReadMUTF8String());
                }
            });
        }

        private void ReadMap(BinaryReader reader)
        {
            PreserveCurrentPosition(reader, MapOffset, () =>
            {
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
            });
        }

        private void ReadHeader(BinaryReader reader)
        {
            Magic = reader.ReadBytes(8);
            FormatChecker.CheckExpression(() => Magic.Match(DexFileMagic, 0));

            CheckSum = reader.ReadUInt32();
            Signature = reader.ReadBytes(20);

            FileSize = reader.ReadUInt32();
            HeaderSize = reader.ReadUInt32();
            EndianTag = reader.ReadUInt32();
            FormatChecker.CheckExpression(() => EndianTag == Endian);

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
