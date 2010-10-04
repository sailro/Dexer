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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Dexer.Core;
using Dexer.Extensions;
using Dexer.Instructions;
using Dexer.Metadata;

namespace Dexer.IO
{
    public class DexReader
    {
        private Dex Dex { get; set; }
        internal Map Map { get; set; }
        internal DexHeader Header { get; set; }

        public DexReader(Dex dex)
        {
            Dex = dex;
            Map = new Map();
            Header = new DexHeader();
        }

        #region " Header "
        private void ReadHeader(BinaryReader reader)
        {
            Header.Magic = reader.ReadBytes(DexConsts.FileMagic.Length);

            if (!Header.Magic.Match(DexConsts.FileMagic, 0))
                throw new MalformedException("Unexpected Magic number");


            Header.CheckSum = reader.ReadUInt32();
            Header.Signature = reader.ReadBytes(DexConsts.SignatureSize);

            Header.FileSize = reader.ReadUInt32();
            Header.HeaderSize = reader.ReadUInt32();
            Header.EndianTag = reader.ReadUInt32();

            if (Header.EndianTag != DexConsts.Endian)
                throw new MalformedException("Only Endian-encoded files are supported");

            Header.LinkSize = reader.ReadUInt32();
            Header.LinkOffset = reader.ReadUInt32();

            Header.MapOffset = reader.ReadUInt32();

            Header.StringsSize = reader.ReadUInt32();
            Header.StringsOffset = reader.ReadUInt32();

            Header.TypeReferencesSize = reader.ReadUInt32();
            Header.TypeReferencesOffset = reader.ReadUInt32();

            Header.PrototypesSize = reader.ReadUInt32();
            Header.PrototypesOffset = reader.ReadUInt32();

            Header.FieldReferencesSize = reader.ReadUInt32();
            Header.FieldReferencesOffset = reader.ReadUInt32();

            Header.MethodReferencesSize = reader.ReadUInt32();
            Header.MethodReferencesOffset = reader.ReadUInt32();

            Header.ClassDefinitionsSize = reader.ReadUInt32();
            Header.ClassDefinitionsOffset = reader.ReadUInt32();

            Header.DataSize = reader.ReadUInt32();
            Header.DataOffset = reader.ReadUInt32();
        }
        #endregion

        #region " MapList "
        private void ReadMapList(BinaryReader reader)
        {
            reader.PreserveCurrentPosition(Header.MapOffset, () =>
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

        #endregion

        #region " Strings "
        private void ReadStrings(BinaryReader reader)
        {
            reader.PreserveCurrentPosition(Header.StringsOffset, () =>
            {
                uint StringsDataOffset = reader.ReadUInt32();
                reader.BaseStream.Seek(StringsDataOffset, SeekOrigin.Begin);
                for (int i = 0; i < Header.StringsSize; i++)
                {
                    Dex.Strings.Add(reader.ReadMUTF8String());
                }
            });
        }
        #endregion

        #region " Collect and sort "
        private void Hierarchicalize()
        {
            var TopClasses = new List<ClassDefinition>();
            foreach (var cdef in Dex.Classes)
            {
                if (cdef.Fullname.Contains(DexConsts.InnerClassMarker.ToString()))
                {
                    String[] items = cdef.Fullname.Split(DexConsts.InnerClassMarker);
                    string fullname = items[0];
                    string name = items[1];
                    ClassDefinition owner = Dex.GetClass(fullname);
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
            Dex.Classes = TopClasses;
        }
        #endregion

        #region " Prefetch "
        private void PrefetchTypeReferences(BinaryReader reader)
        {
            reader.PreserveCurrentPosition(Header.TypeReferencesOffset, () =>
            {
                reader.BaseStream.Seek(Header.TypeReferencesOffset, SeekOrigin.Begin);

                for (int i = 0; i < Header.TypeReferencesSize; i++)
                {
                    int descriptorIndex = reader.ReadInt32();
                    string descriptor = Dex.Strings[descriptorIndex];
                    Dex.TypeReferences.Add(TypeDescriptor.Allocate(descriptor));
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
                FieldDefinition fdef = new FieldDefinition(Dex.FieldReferences[fieldIndex]);
                Dex.FieldReferences[fieldIndex] = fdef;
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
                MethodDefinition mdef = new MethodDefinition(Dex.MethodReferences[methodIndex]);
                Dex.MethodReferences[methodIndex] = mdef;
            }
        }

        private void PrefetchClassDefinition(BinaryReader reader, ClassDefinition classDefinition, uint classDataOffset)
        {
            reader.PreserveCurrentPosition(classDataOffset, () =>
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
            reader.PreserveCurrentPosition(Header.ClassDefinitionsOffset, () =>
            {
                for (int i = 0; i < Header.ClassDefinitionsSize; i++)
                {
                    int classIndex = reader.ReadInt32();

                    ClassDefinition cdef;
                    if (Dex.TypeReferences[classIndex] is ClassDefinition)
                    {
                        cdef = (ClassDefinition)Dex.TypeReferences[classIndex];
                    }
                    else
                    {
                        cdef = new ClassDefinition((ClassReference)Dex.TypeReferences[classIndex]);
                        Dex.TypeReferences[classIndex] = cdef;
                        Dex.Classes.Add(cdef);
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

        #region " Annotations "
        private void ReadAnnotationDirectory(BinaryReader reader, ClassDefinition classDefinition, uint annotationOffset)
        {
            reader.PreserveCurrentPosition(annotationOffset, () =>
            {
                uint classAnnotationOffset = reader.ReadUInt32();
                uint annotatedFieldsSize = reader.ReadUInt32();
                uint annotatedMethodsSize = reader.ReadUInt32();
                uint annotatedParametersSize = reader.ReadUInt32();

                if (classAnnotationOffset > 0)
                    classDefinition.Annotations = ReadAnnotationSet(reader, classAnnotationOffset);

                for (int j = 0; j < annotatedFieldsSize; j++)
                    (Dex.FieldReferences[reader.ReadInt32()] as FieldDefinition).Annotations = ReadAnnotationSet(reader, reader.ReadUInt32());

                for (int j = 0; j < annotatedMethodsSize; j++)
                    (Dex.MethodReferences[reader.ReadInt32()] as MethodDefinition).Annotations = ReadAnnotationSet(reader, reader.ReadUInt32());

                for (int j = 0; j < annotatedParametersSize; j++)
                {
                    int methodIndex = reader.ReadInt32();
                    uint offset = reader.ReadUInt32();
                    var annotations = ReadAnnotationSetRefList(reader, offset);
                    MethodDefinition mdef = (Dex.MethodReferences[methodIndex] as MethodDefinition);

                    for (int i = 0; i < annotations.Count; i++)
                    {
                        if (annotations[i].Count > 0)
                        {
                            mdef.Prototype.Parameters[i].Annotations = annotations[i];
                        }
                    }
                }
            });
        }

        private List<List<Annotation>> ReadAnnotationSetRefList(BinaryReader reader, uint annotationOffset)
        {
            var result = new List<List<Annotation>>();
            reader.PreserveCurrentPosition(annotationOffset, () =>
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

        private List<Annotation> ReadAnnotationSet(BinaryReader reader, uint annotationOffset)
        {
            var result = new List<Annotation>();
            reader.PreserveCurrentPosition(annotationOffset, () =>
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
            reader.PreserveCurrentPosition(annotationOffset, () =>
            {
                byte visibility = reader.ReadByte();
                annotation = ReadEncodedAnnotation(reader);
                annotation.Visibility = (AnnotationVisibility)visibility;
            });
            return annotation;
        }

        private Annotation ReadEncodedAnnotation(BinaryReader reader)
        {
            int typeIndex = (int)reader.ReadULEB128();
            int elementSize = (int)reader.ReadULEB128();

            Annotation annotation = new Annotation();
            annotation.Type = (ClassReference)Dex.TypeReferences[typeIndex];

            for (int i = 0; i < elementSize; i++)
            {
                AnnotationArgument argument = new AnnotationArgument();
                int nameIndex = (int)reader.ReadULEB128();
                argument.Name = Dex.Strings[nameIndex];
                argument.Value = ReadValue(reader);
                annotation.Arguments.Add(argument);
            }

            return annotation;
        }
        #endregion

        #region " Prototypes "
        private void ReadPrototypes(BinaryReader reader)
        {
            reader.PreserveCurrentPosition(Header.PrototypesOffset, () =>
            {
                for (int i = 0; i < Header.PrototypesSize; i++)
                {
                    long thisOffset = reader.BaseStream.Position;
                    int shortyIndex = reader.ReadInt32();
                    int returnTypeIndex = reader.ReadInt32();
                    uint parametersOffset = reader.ReadUInt32();

                    Prototype prototype = new Prototype();
                    prototype.ReturnType = Dex.TypeReferences[returnTypeIndex];

                    if (parametersOffset > 0)
                    {
                        ReadParameters(reader, prototype, parametersOffset);
                    }

                    Dex.Prototypes.Add(prototype);
                }
            });
        }

        private void ReadParameters(BinaryReader reader, Prototype prototype, uint parametersOffset)
        {
            reader.PreserveCurrentPosition(parametersOffset, () =>
            {
                uint typecount = reader.ReadUInt32();
                for (int j = 0; j < typecount; j++)
                {
                    Parameter parameter = new Parameter();
                    ushort typeIndex = reader.ReadUInt16();
                    parameter.Type = Dex.TypeReferences[typeIndex];
                    prototype.Parameters.Add(parameter);
                }
            });
        }
        #endregion

        #region " Definitions "
        private void ReadClassDefinitions(BinaryReader reader)
        {
            reader.PreserveCurrentPosition(Header.ClassDefinitionsOffset, () =>
            {
                for (int i = 0; i < Header.ClassDefinitionsSize; i++)
                {
                    uint classIndex = reader.ReadUInt32();

                    ClassDefinition cdef = (ClassDefinition)Dex.TypeReferences[(int)classIndex];
                    cdef.AccessFlags = (AccessFlags)reader.ReadUInt32();

                    uint superClassIndex = reader.ReadUInt32();
                    if (superClassIndex != DexConsts.NoIndex)
                        cdef.SuperClass = (ClassReference)Dex.TypeReferences[(int)superClassIndex];

                    uint interfaceOffset = reader.ReadUInt32();
                    uint sourceFileIndex = reader.ReadUInt32();
                    uint annotationOffset = reader.ReadUInt32();
                    uint classDataOffset = reader.ReadUInt32();
                    uint staticValuesOffset = reader.ReadUInt32();

                    if (interfaceOffset > 0)
                        ReadInterfaces(reader, cdef, interfaceOffset);

                    if (sourceFileIndex != DexConsts.NoIndex)
                        cdef.SourceFile = Dex.Strings[(int)sourceFileIndex];

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
            reader.PreserveCurrentPosition(staticValuesOffset, () =>
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
            reader.PreserveCurrentPosition(interfaceOffset, () =>
            {
                uint typecount = reader.ReadUInt32();
                for (int j = 0; j < typecount; j++)
                {
                    ushort typeIndex = reader.ReadUInt16();
                    classDefinition.Interfaces.Add((ClassReference)Dex.TypeReferences[typeIndex]);
                }
            });
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
                    return (short)reader.ReadByByteLength(valueArgument + 1);
                case ValueFormats.Char:
                    return (char)reader.ReadByByteLength(valueArgument + 1);
                case ValueFormats.Int:
                    return (int)reader.ReadByByteLength(valueArgument + 1);
                case ValueFormats.Long:
                    return (long)reader.ReadByByteLength(valueArgument + 1);
                case ValueFormats.Float:
                    return reader.ReadSingle();
                case ValueFormats.Double:
                    return reader.ReadDouble();
                case ValueFormats.String:
                    return Dex.Strings[(int)reader.ReadByByteLength(valueArgument + 1)];
                case ValueFormats.Type:
                    return Dex.TypeReferences[(int)reader.ReadByByteLength(valueArgument + 1)];
                case ValueFormats.Field:
                case ValueFormats.Enum:
                    return Dex.FieldReferences[(int)reader.ReadByByteLength(valueArgument + 1)];
                case ValueFormats.Method:
                    return Dex.MethodReferences[(int)reader.ReadByByteLength(valueArgument + 1)];
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
            uint fieldIndex = 0;
            for (int i = 0; i < fieldcount; i++)
            {
                fieldIndex += reader.ReadULEB128();
                uint accessFlags = reader.ReadULEB128();

                FieldDefinition fdef = (FieldDefinition)Dex.FieldReferences[(int)fieldIndex];
                fdef.AccessFlags = (AccessFlags)accessFlags;
                fdef.Owner = classDefinition;

                classDefinition.Fields.Add(fdef);
            }
        }

        private void ReadMethodDefinitions(BinaryReader reader, ClassDefinition classDefinition, uint methodcount, bool isVirtual)
        {
            uint methodIndex = 0;
            for (int i = 0; i < methodcount; i++)
            {
                methodIndex += reader.ReadULEB128();
                uint accessFlags = reader.ReadULEB128();
                uint codeOffset = reader.ReadULEB128();

                MethodDefinition mdef = (MethodDefinition)Dex.MethodReferences[(int)methodIndex];
                mdef.AccessFlags = (AccessFlags)accessFlags;
                mdef.Owner = classDefinition;
                mdef.IsVirtual = isVirtual;

                classDefinition.Methods.Add(mdef);

                if (codeOffset > 0)
                    ReadMethodBody(reader, mdef, codeOffset);
            }
        }

        private void ReadMethodBody(BinaryReader reader, MethodDefinition mdef, uint codeOffset)
        {
            reader.PreserveCurrentPosition(codeOffset, () =>
            {
                ushort registersSize = reader.ReadUInt16();
                ushort incomingArgsSize = reader.ReadUInt16();
                ushort outgoingArgsSize = reader.ReadUInt16();
                ushort triesSize = reader.ReadUInt16();
                uint debugOffset = reader.ReadUInt32();

                mdef.Body = new MethodBody(mdef, registersSize);
                mdef.Body.IncomingArguments = incomingArgsSize;
                mdef.Body.OutgoingArguments = outgoingArgsSize;

                InstructionReader ireader = new InstructionReader(Dex, mdef);
                ireader.ReadFrom(reader);

                if ((triesSize != 0) && (ireader.Codes.Length % 2 != 0))
                    reader.ReadUInt16(); // padding (4-byte alignment)

                if (triesSize != 0)
                    ReadExceptionHandlers(reader, mdef, ireader, triesSize);

                if (debugOffset != 0)
                    ReadDebugInfo(reader, mdef, ireader, debugOffset);
            });

        }

        private void ReadDebugInfo(BinaryReader reader, MethodDefinition mdef, InstructionReader instructionReader, uint debugOffset)
        {
            reader.PreserveCurrentPosition(debugOffset, () =>
            {
                DebugInfo debugInfo = new DebugInfo(mdef.Body);
                mdef.Body.DebugInfo = debugInfo;

                uint lineStart = reader.ReadULEB128();
                debugInfo.LineStart = lineStart;

                uint parametersSize = reader.ReadULEB128();
                for (int i = 0; i < parametersSize; i++)
                {
                    long index = reader.ReadULEB128p1();
                    string name = null;
                    if (index != DexConsts.NoIndex && index >= 0)
                        name = Dex.Strings[(int)index];
                    debugInfo.Parameters.Add(name);
                }

                while (true)
                {
                    DebugInstruction ins = new DebugInstruction();
                    ins.OpCode = (DebugOpCodes)reader.ReadByte();
                    debugInfo.DebugInstructions.Add(ins);

                    uint registerIndex;
                    uint addrDiff;
                    long nameIndex;
                    long typeIndex;
                    long signatureIndex;
                    int lineDiff;
                    string name;

                    switch (ins.OpCode)
                    {
                        case DebugOpCodes.AdvancePc:
                            // uleb128 addr_diff
                            addrDiff = reader.ReadULEB128();
                            ins.Operands.Add(addrDiff);
                            break;
                        case DebugOpCodes.AdvanceLine:
                            // sleb128 line_diff
                            lineDiff = reader.ReadSLEB128();
                            ins.Operands.Add(lineDiff);
                            break;
                        case DebugOpCodes.EndLocal:
                        case DebugOpCodes.RestartLocal:
                            // uleb128 register_num
                            registerIndex = reader.ReadULEB128();
                            ins.Operands.Add(mdef.Body.Registers[(int)registerIndex]);
                            break;
                        case DebugOpCodes.SetFile:
                            // uleb128p1 name_idx
                            nameIndex = reader.ReadULEB128p1();
                            name = null;
                            if (nameIndex != DexConsts.NoIndex && nameIndex >= 0)
                                name = Dex.Strings[(int)nameIndex];
                            ins.Operands.Add(name);
                            break;
                        case DebugOpCodes.StartLocalExtended:
                        case DebugOpCodes.StartLocal:
                            // StartLocalExtended : uleb128 register_num, uleb128p1 name_idx, uleb128p1 type_idx, uleb128p1 sig_idx
                            // StartLocal : uleb128 register_num, uleb128p1 name_idx, uleb128p1 type_idx
                            Boolean isExtended = ins.OpCode == DebugOpCodes.StartLocalExtended;

                            registerIndex = reader.ReadULEB128();
                            ins.Operands.Add(mdef.Body.Registers[(int)registerIndex]);

                            nameIndex = reader.ReadULEB128p1();
                            name = null;
                            if (nameIndex != DexConsts.NoIndex && nameIndex >= 0)
                                name = Dex.Strings[(int)nameIndex];
                            ins.Operands.Add(name);

                            typeIndex = reader.ReadULEB128p1();
                            TypeReference type = null;
                            if (typeIndex != DexConsts.NoIndex && typeIndex >= 0)
                                type = Dex.TypeReferences[(int)typeIndex];
                            ins.Operands.Add(type);

                            if (isExtended)
                            {
                                signatureIndex = reader.ReadULEB128p1();
                                string signature = null;
                                if (signatureIndex != DexConsts.NoIndex && signatureIndex >= 0)
                                    signature = Dex.Strings[(int)signatureIndex];
                                ins.Operands.Add(signature);
                            }

                            break;
                        case DebugOpCodes.EndSequence:
                            return;
                        case DebugOpCodes.Special:
                        // between 0x0a and 0xff (inclusive)
                        case DebugOpCodes.SetPrologueEnd:
                        case DebugOpCodes.SetEpilogueBegin:
                        default:
                            break;
                    }
                }
            });
        }

        private void ReadExceptionHandlers(BinaryReader reader, MethodDefinition mdef, InstructionReader instructionReader, ushort triesSize)
        {
            var exceptionLookup = new Dictionary<uint, List<ExceptionHandler>>();
            for (int i = 0; i < triesSize; i++)
            {
                uint startOffset = reader.ReadUInt32();
                uint insCount = reader.ReadUInt16();
                uint endOffset = startOffset + insCount - 1;
                uint handlerOffset = reader.ReadUInt16();

                ExceptionHandler ehandler = new ExceptionHandler();
                mdef.Body.Exceptions.Add(ehandler);
                if (!exceptionLookup.ContainsKey(handlerOffset))
                {
                    exceptionLookup.Add(handlerOffset, new List<ExceptionHandler>());
                }
                exceptionLookup[handlerOffset].Add(ehandler);
                ehandler.TryStart = instructionReader.Lookup[(int)startOffset];
                // The last code unit covered (inclusive) is start_addr + insn_count - 1
                ehandler.TryEnd = instructionReader.LookupLast[(int)endOffset];
            }

            long baseOffset = reader.BaseStream.Position;
            uint catchHandlersSize = reader.ReadULEB128();
            for (int i = 0; i < catchHandlersSize; i++)
            {
                long itemoffset = reader.BaseStream.Position - baseOffset;
                int catchTypes = reader.ReadSLEB128();
                bool catchAllPresent = catchTypes <= 0;
                catchTypes = Math.Abs(catchTypes);

                for (int j = 0; j < catchTypes; j++)
                {
                    uint typeIndex = reader.ReadULEB128();
                    uint offset = reader.ReadULEB128();
                    Catch @catch = new Catch();
                    @catch.Type = Dex.TypeReferences[(int)typeIndex];
                    @catch.Instruction = instructionReader.Lookup[(int)offset];

                    // As catch handler can be used in several tries, let's clone the catch
                    foreach (ExceptionHandler ehandler in exceptionLookup[(uint)itemoffset])
                        ehandler.Catches.Add(@catch.Clone());
                }

                if (catchAllPresent)
                {
                    uint offset = reader.ReadULEB128();
                    foreach (ExceptionHandler ehandler in exceptionLookup[(uint)itemoffset])
                        ehandler.CatchAll = instructionReader.Lookup[(int)offset];
                }

            }
        }

        private void ReadClassDefinition(BinaryReader reader, ClassDefinition classDefinition, uint classDataOffset)
        {
            reader.PreserveCurrentPosition(classDataOffset, () =>
            {
                uint staticFieldSize = reader.ReadULEB128();
                uint instanceFieldSize = reader.ReadULEB128();
                uint directMethodSize = reader.ReadULEB128();
                uint virtualMethodSize = reader.ReadULEB128();

                ReadFieldDefinitions(reader, classDefinition, staticFieldSize);
                ReadFieldDefinitions(reader, classDefinition, instanceFieldSize);
                ReadMethodDefinitions(reader, classDefinition, directMethodSize, false);
                ReadMethodDefinitions(reader, classDefinition, virtualMethodSize, true);
            });
        }
        #endregion

        #region " References "
        private void ReadMethodReferences(BinaryReader reader)
        {
            reader.PreserveCurrentPosition(Header.MethodReferencesOffset, () =>
            {
                for (int i = 0; i < Header.MethodReferencesSize; i++)
                {
                    int classIndex = reader.ReadUInt16();
                    int prototypeIndex = reader.ReadUInt16();
                    int nameIndex = reader.ReadInt32();

                    MethodReference mref = new MethodReference();
                    mref.Owner = (CompositeType) Dex.TypeReferences[classIndex]; 
                    // Clone the prototype so we can annotate & update it easily
                    mref.Prototype = Dex.Prototypes[prototypeIndex].Clone();
                    mref.Name = Dex.Strings[nameIndex];

                    Dex.MethodReferences.Add(mref);
                }
            });
        }

        private void ReadFieldReferences(BinaryReader reader)
        {
            reader.PreserveCurrentPosition(Header.FieldReferencesOffset, () =>
            {
                for (int i = 0; i < Header.FieldReferencesSize; i++)
                {
                    int classIndex = reader.ReadUInt16();
                    int typeIndex = reader.ReadUInt16();
                    int nameIndex = reader.ReadInt32();

                    FieldReference fref = new FieldReference();

                    fref.Owner = (ClassReference) Dex.TypeReferences[classIndex];
                    fref.Type = Dex.TypeReferences[typeIndex];
                    fref.Name = Dex.Strings[nameIndex];

                    Dex.FieldReferences.Add(fref);
                }
            });
        }

        private void ReadTypesReferences(BinaryReader reader)
        {
            reader.PreserveCurrentPosition(Header.TypeReferencesOffset, () =>
            {
                for (int i = 0; i < Header.TypeReferencesSize; i++)
                {
                    int descriptorIndex = reader.ReadInt32();
                    string descriptor = Dex.Strings[descriptorIndex];
                    TypeDescriptor.Fill(descriptor, Dex.TypeReferences[i], Dex);
                }
            });
        }
        #endregion

        public void ReadFrom(BinaryReader reader)
        {
            ReadHeader(reader);
            ReadMapList(reader);
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

    }
}
